using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BuildingCoder;
using IFCtoRevit.Base;
using IFCtoRevit.IFCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace IFCtoRevit.ViewModels
{
	public partial class MainWindowViewModel : ViewModelBase
	{

		#region Fields

		public static int RevitVersion;

		List<FamilyInstance> colInstances;
		List<FamilyInstance> incInstances;
		List<FamilyInstance> beamInstances;
		List<Grid>[] grids;
		List<Level> levels;
		List<Floor> floors;
		List<FamilyInstance> colStInstances;
		List<FamilyInstance> BraceInstances;
		List<FamilyInstance> beamStInstances;


		int lvlNumber = 0;
		byte xGridName = 65; //ASCII code of 'A'
		int yGridName = 1;

		#endregion
		   
		#region Transaction

		public void InitTransaction(Document doc, string transactionName, Action action)
		{
			using (Transaction t = new Transaction(doc, transactionName))
			{
				t.Start();
				action();
				t.Commit();
			}
		}

		#endregion

		#region Grids

		public List<XYZ> GetGridsIntersections(List<FamilyInstance> colInstances)
		{
			List<XYZ> colsLocationPts = new List<XYZ>();
			colInstances.ForEach(col => colsLocationPts.Add((col.Location as LocationPoint).Point));
			return colsLocationPts.GroupBy(pt => new
			{
				X = Math.Round(pt.X, 5, MidpointRounding.AwayFromZero),
				Y = Math.Round(pt.Y, 5, MidpointRounding.AwayFromZero)
			}).Select(g => g.First()).ToList();

		}

		public List<double> GetXGridsLocations(List<XYZ> gridsIntersections)
		{
			return gridsIntersections.GroupBy(pt => Math.Round(pt.Y, 5, MidpointRounding.AwayFromZero))
									 .Select(g => g.First().Y)
									 .ToList();
		}

		public List<double> GetYGridsLocations(List<XYZ> gridsIntersections)
		{
			return gridsIntersections.GroupBy(pt => Math.Round(pt.X, 5, MidpointRounding.AwayFromZero))
									 .Select(g => g.First().X)
									 .ToList();
		}

		public List<Grid>[] CreateGrids(Document doc, List<double> xLocations, List<double> yLocations)
		{

			List<Grid> xGrids = xLocations.Select(yCoord => Line.CreateBound(new XYZ(yLocations.Min() - Util.MmToFoot(2000), yCoord, 0),
																		 new XYZ(yLocations.Max() + Util.MmToFoot(2000), yCoord, 0)))
											.Select(line => Grid.Create(doc, line)).ToList();

			xGrids.ForEach(grid => grid.Name = Encoding.ASCII.GetString(new byte[1] { xGridName++ }));


			List<Grid> yGrids = yLocations.Select(xCoord => Line.CreateBound(new XYZ(xCoord, xLocations.Min() - Util.MmToFoot(2000), 0),
																		 new XYZ(xCoord, xLocations.Max() + Util.MmToFoot(2000), 0))).ToList()
											.Select(line => Grid.Create(doc, line)).ToList();

			yGrids.ForEach(grid => grid.Name = (yGridName++).ToString());

			return new List<Grid>[2] { xGrids, yGrids };

		}
		#endregion

		#region RC Columns

		public FamilySymbol GetColumnFamilySymbol(Document doc, Column col, string familyName, BuiltInCategory category)
		{
			string b = category == BuiltInCategory.OST_StructuralColumns ? "b" : "Width";
			string h = category == BuiltInCategory.OST_StructuralColumns ? "h" : "Depth";

			FamilySymbol fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.Where(f => f.FamilyName == familyName)
														.FirstOrDefault(f =>
														{
															if (f.LookupParameter(h).AsDouble() == Util.MmToFoot(col.Depth) &&
																	f.LookupParameter(b).AsDouble() == Util.MmToFoot(col.Width))
															{
																return true;
															}
															return false;
														});

			if (fs != null) return fs;

			Family family = default;
			try
			{
				fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.First(f => f.FamilyName == familyName);
			}
			catch (Exception)
			{
				string directory = $"C:/ProgramData/Autodesk/RVT {RevitVersion}/Libraries/US Imperial/Structural Columns/Concrete/";
				family = OpenFamily(doc, directory, familyName);
				fs = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
			}

			ElementType fs1 = fs.Duplicate(String.Format("Column {0:0}x{1:0}", col.Width / 10, col.Depth / 10));
			fs1.LookupParameter(b).Set(Util.MmToFoot(col.Width));
			fs1.LookupParameter(h).Set(Util.MmToFoot(col.Depth));
			return fs1 as FamilySymbol;
		}


		public Family OpenFamily(Document doc, string directory, string familyName)
		{
			string path = directory + familyName + ".rfa";

			Func<Element, bool> nameEquals = e => e.Name.Equals(familyName);

			FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(Family));

			Family f = collector.Any<Element>(nameEquals) ? collector.First<Element>(nameEquals) as Family : null;

			if (f == null) doc.LoadFamily(path, out f);

			return f;
		}

		public FamilyInstance CreateColumnInstance(Document doc, XYZ location, Level bottomLvl, Level topLvl, FamilySymbol colType, double rotationAngleRad, bool isStructural)
		{

			FamilyInstance col;
			StructuralType structuralType = isStructural ? StructuralType.Column : StructuralType.NonStructural;
			if (!colType.IsActive) colType.Activate();
			col = doc.Create.NewFamilyInstance(location, colType, bottomLvl, structuralType);
			col.LookupParameter("Top Level").Set(topLvl.Id);
			col.LookupParameter("Top Offset").Set(0);
			col.LookupParameter("Base Offset").Set(0);
			ElementTransformUtils.RotateElement(doc, col.Id, Line.CreateBound(location, location + XYZ.BasisZ), rotationAngleRad);


			return col;
		}

		public FamilyInstance CreateColumnInstance(Document doc, Column column, FamilySymbol colType, bool isStructural, List<double> storeys)
		{

			XYZ location = new XYZ(Util.MmToFoot(column.Location.X), Util.MmToFoot(column.Location.Y), 0);
			Level colBottomLevel = GetLevel(doc, MapToStorey(column.BottomLevel, storeys));
			Level colTopLevel = GetLevel(doc, MapToStorey(column.TopLevel, storeys));

			double colRotation = column.RefDirection.X >= 0.0 ?
				90 + Vector3D.AngleBetween(new Vector3D(column.RefDirection.X, column.RefDirection.Y, column.RefDirection.Z), new Vector3D(0, -1, 0)) :
				90 + Vector3D.AngleBetween(new Vector3D(column.RefDirection.X, column.RefDirection.Y, column.RefDirection.Z), new Vector3D(0, -1, 0));

			colRotation *= Math.PI / 180.0;

			return CreateColumnInstance(doc, location, colBottomLevel, colTopLevel, colType, colRotation, isStructural);

		}

		public List<FamilyInstance> CreateColumnInstance(Document doc, List<Column> cols, string familyName, bool isStructural, List<double> storeys)
		{
			FamilySymbol colType;
			return cols.Select(col =>
			{
				colType = GetColumnFamilySymbol(doc, col, familyName, BuiltInCategory.OST_StructuralColumns);
				return CreateColumnInstance(doc, col, colType, isStructural, storeys);

			}).ToList();
		}

		#endregion

		#region Slanted RC Columns
		public FamilySymbol GetInclinesFamilySymbol(Document doc, Inclined inclined, string familyName, BuiltInCategory category)
		{
			string b = category == BuiltInCategory.OST_StructuralColumns ? "b" : "Width";
			string h = category == BuiltInCategory.OST_StructuralColumns ? "h" : "Depth";

			FamilySymbol fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.Where(f => f.FamilyName == familyName)
														.FirstOrDefault(f =>
														{
															if (f.LookupParameter(h).AsDouble() == Util.MmToFoot(inclined.Depth) &&
																	f.LookupParameter(b).AsDouble() == Util.MmToFoot(inclined.Width))
															{
																return true;
															}
															return false;
														});

			if (fs != null) return fs;

			Family family = default;
			try
			{
				fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.First(f => f.FamilyName == familyName);
			}
			catch (Exception)
			{
				string directory = $"C:/ProgramData/Autodesk/RVT {RevitVersion}/Libraries/US Imperial/Structural Columns/Concrete/";
				family = OpenFamily(doc, directory, familyName);
				fs = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
			}

			ElementType fs1 = fs.Duplicate(String.Format("Column {0:0}x{1:0}", inclined.Width / 10, inclined.Depth / 10));
			fs1.LookupParameter(b).Set(Util.MmToFoot(inclined.Width));
			fs1.LookupParameter(h).Set(Util.MmToFoot(inclined.Depth));
			return fs1 as FamilySymbol;
		}

		public FamilyInstance CreateInclinedInstance(Document doc, XYZ location, XYZ endLocation, Level bottomLvl, Level topLvl, FamilySymbol colType, double rotationAngleRad, bool isStructural)
		{

			FamilyInstance inclined;
			StructuralType structuralType = isStructural ? StructuralType.Column : StructuralType.NonStructural;
			if (!colType.IsActive) colType.Activate();
			Line L1 = Line.CreateBound(location, endLocation);
			inclined = doc.Create.NewFamilyInstance(L1, colType, bottomLvl, structuralType);
			inclined.LookupParameter("Top Level").Set(topLvl.Id);
			inclined.LookupParameter("Top Offset").Set(0);
			inclined.LookupParameter("Base Offset").Set(0);
			//ElementTransformUtils.RotateElement(doc, inclined.Id, Line.CreateBound(location, endLocation), rotationAngleRad);


			return inclined;
		}

		public FamilyInstance CreateInclinedInstance(Document doc, Inclined inclined, FamilySymbol colType, bool isStructural, List<double> storeys)
		{
			XYZ location = new XYZ(Util.MmToFoot(inclined.Location.X), Util.MmToFoot(inclined.Location.Y), Util.MmToFoot(inclined.Location.Z));
			XYZ endLocation = location + Util.MmToFoot(inclined.Length) * new XYZ(inclined.Axis.X, inclined.Axis.Y, inclined.Axis.Z);

			Level colBottomLevel = GetLevel(doc, MapToStorey(inclined.BottomLevel, storeys));
			double toplevel = Util.FootToMm(endLocation.Z);
			Level colTopLevel = GetLevel(doc, MapToStorey(toplevel, storeys));

			double colRotation = inclined.RefDirection.X >= 0.0 ?
				90 + Vector3D.AngleBetween(new Vector3D(inclined.RefDirection.X, inclined.RefDirection.Y, inclined.RefDirection.Z), new Vector3D(0, -1, 0)) :
				90 + Vector3D.AngleBetween(new Vector3D(inclined.RefDirection.X, inclined.RefDirection.Y, inclined.RefDirection.Z), new Vector3D(0, -1, 0));

			colRotation *= Math.PI / 180.0;

			return CreateInclinedInstance(doc, location, endLocation, colBottomLevel, colTopLevel, colType, colRotation, isStructural);
		}

		public List<FamilyInstance> CreateInclinedInstance(Document doc, List<Inclined> incs, string familyName, bool isStructural, List<double> storeys)
		{
			FamilySymbol incType;
			return incs.Select(inc =>
			{
				incType = GetInclinesFamilySymbol(doc, inc, familyName, BuiltInCategory.OST_StructuralColumns);
				return CreateInclinedInstance(doc, inc, incType, isStructural, storeys);

			}).ToList();
		}

		#endregion

		#region RC Beams

		public FamilyInstance CreateBeamInstance(Document doc, Beam beam, FamilySymbol fs)
		{
			StructuralType stBeam = StructuralType.Beam;
			XYZ p1 = new XYZ(Util.MmToFoot(beam.Location.X), Util.MmToFoot(beam.Location.Y), Util.MmToFoot(beam.Location.Z));
			XYZ p2 = p1 + beam.Length * new XYZ(Util.MmToFoot(beam.Axis.X), Util.MmToFoot(beam.Axis.Y), Util.MmToFoot(beam.Axis.Z));
			Line l1 = Line.CreateBound(p1, p2);
			FamilyInstance fi = doc.Create.NewFamilyInstance(l1, fs, null, stBeam);
			fi.LookupParameter("Reference Level").Set(GetLevel(doc, beam.Location.Z).Id);
			return fi;
		}
		public List<FamilyInstance> CreateBeamInstance(Document doc, List<Beam> beams, string familyName)
		{
			return beams.Select(x => CreateBeamInstance(doc, x, GetBeamFamilySymbol(doc, x, familyName, BuiltInCategory.OST_StructuralFraming))).ToList();
		}

		public FamilySymbol GetBeamFamilySymbol(Document doc, Beam beam, string familyName, BuiltInCategory category)
		{
			FamilySymbol fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.Where(f => f.FamilyName == familyName)
														.FirstOrDefault(f =>
														{
															if (Math.Abs(f.LookupParameter("h").AsDouble() - Util.MmToFoot(beam.H)) < Util.MmToFoot(0.001) &&
																Math.Abs(f.LookupParameter("b").AsDouble() - Util.MmToFoot(beam.B)) < Util.MmToFoot(0.001))

																return true;
															return false;
														});

			if (fs != null) return fs;
			Family family = default;

			try
			{
				fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.First(f => f.FamilyName == familyName);

			}
			catch (Exception)
			{
				string directory = $"C:/ProgramData/Autodesk/RVT {RevitVersion}/Libraries/US Imperial/Structural Framing/Concrete/";
				family = OpenFamily(doc, directory, familyName);
				fs = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;

			}
			ElementType fs1 = fs.Duplicate(String.Format("Beam {0:0}x{1:0}", beam.B / 10, beam.H / 10));
			fs1.LookupParameter("h").Set(Util.MmToFoot(beam.H));
			fs1.LookupParameter("b").Set(Util.MmToFoot(beam.B));
			return fs1 as FamilySymbol;
		}

		#endregion

		#region Levels
		public List<Level> CreateLevels(Document doc, List<double> storeys)
		{
			return storeys.Select(storey =>
			{
				Level lvl = Level.Create(doc, Util.MmToFoot(storey));
				lvl.LookupParameter("Name").Set(String.Format("Level {0}", lvlNumber++));
				return lvl;
			}).ToList();
		}

		public Level GetLevel(Document doc, double elevation)
		{
			Level rLevel;

			try
			{
				rLevel = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels)
														.WhereElementIsNotElementType()
														.Cast<Level>()
														.First(lvl => lvl.Elevation == Util.MmToFoot(elevation));
			}
			catch (Exception)
			{
				rLevel = Level.Create(doc, Util.MmToFoot(elevation));
				rLevel.LookupParameter("Name").Set(String.Format("Level {0}", lvlNumber++));
			}

			return rLevel;
		}

		public double MapToStorey(double level, List<double> storeys)
		{
			Dictionary<int, double> def = new Dictionary<int, double>();
			int i = 0;
			foreach (double st in storeys)
			{
				def.Add(i, Math.Abs(level - st));
				i++;
			}
			return storeys[def.OrderBy(kvp => kvp.Value).First().Key];
		}

		#endregion

		#region Floors

		public Floor CreateFloor(Document doc, FloorSlab floor)
		{
			CurveArray curArr = new CurveArray();

			List<XYZ> pts = floor.Profile.Select(pt =>
			{
				Point3D pt1 = new Point3D(pt.X, pt.Y, pt.Z) * floor.Mat;
				return new XYZ(Util.MmToFoot(pt1.X), Util.MmToFoot(pt1.Y), Util.MmToFoot(pt1.Z));

			}).ToList();
			for (int i = 0; i < pts.Count - 1; i++)
			{
				curArr.Append(Line.CreateBound(pts[i], pts[i + 1]));
			}

			Floor f = doc.Create.NewFloor(curArr, GetFloorType(doc, floor), GetLevel(doc, floor.Level), false);
			f.LookupParameter("Height Offset From Level").Set(0);

			return f;
		}

		public List<Floor> CreateFloor(Document doc, List<FloorSlab> floors)
		{
			return floors.Select(floor => CreateFloor(doc, floor)).ToList();
		}

		public FloorType GetFloorType(Document doc, FloorSlab floor)
		{
			FloorType f = new FilteredElementCollector(doc).OfClass(typeof(FloorType))
															.Where(ft => ft is FloorType)
															.FirstOrDefault(e =>
															{
																CompoundStructure comp = (e as FloorType).GetCompoundStructure();
																if (comp.GetLayerWidth(0) == Util.MmToFoot(floor.Depth))
																{
																	return true;
																}
																return false;

															}) as FloorType;
			if (f != null) return f;
			f = new FilteredElementCollector(doc).OfClass(typeof(FloorType)).FirstOrDefault(e => e.Name == "Generic - 12\"") as FloorType;
			f = f.Duplicate(String.Format("Floor {0} CM", floor.Depth / 10)) as FloorType;
			CompoundStructure compound = f.GetCompoundStructure();
			compound.SetLayerWidth(0, Util.MmToFoot(floor.Depth));
			f.SetCompoundStructure(compound);

			return f;


		}


		#endregion

		#region Steel Families Symbols Functions

		public FamilySymbol GetIShapeColumnFamilySymbol(Document doc, ColumnSt col, string familyName, BuiltInCategory category)
		{
			FamilySymbol fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.Where(f => f.FamilyName == familyName)
														.FirstOrDefault(f =>
														{
															if (f.LookupParameter("Width").AsDouble() == Util.MmToFoot(col.Width) &&
																f.LookupParameter("Height").AsDouble() == Util.MmToFoot(col.Depth) &&
																f.LookupParameter("Flange Thickness").AsDouble() == Util.MmToFoot(col.FlangeTh) &&
																f.LookupParameter("Web Thickness").AsDouble() == Util.MmToFoot(col.WebTh)
																)
															{
																return true;
															}
															return false;
														});

			if (fs != null) return fs;

			Family family = default;
			try
			{
				fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.First(f => f.FamilyName == familyName);
			}
			catch (Exception)
			{
				string directory = $"C:/ProgramData/Autodesk/RVT {RevitVersion}/Libraries/US Imperial/Structural Columns/Steel/AISC 14.1/";
				family = OpenFamily(doc, directory, familyName);
				fs = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
			}

			ElementType fs1 = fs.Duplicate(String.Format("I-Column Custom {0:0}x{1:0}", col.Width / 10, col.Depth / 10));
			fs1.LookupParameter("Width").Set(Util.MmToFoot(col.Width));
			fs1.LookupParameter("Height").Set(Util.MmToFoot(col.Depth));
			fs1.LookupParameter("Flange Thickness").Set(Util.MmToFoot(col.FlangeTh));
			fs1.LookupParameter("Web Thickness").Set(Util.MmToFoot(col.WebTh));
			fs1.LookupParameter("Web Fillet").Set(Util.MmToFoot(0.85 * (col.WebTh)));

			return fs1 as FamilySymbol;
		}

		public FamilySymbol GetIShapeBeamFamilySymbol(Document doc, BeamSt Beam, string familyName, BuiltInCategory category)
		{
			FamilySymbol fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.Where(f => f.FamilyName == familyName)
														.FirstOrDefault(f =>
														{
															if (f.LookupParameter("Width").AsDouble() == Util.MmToFoot(Beam.Width) &&
																f.LookupParameter("Height").AsDouble() == Util.MmToFoot(Beam.Depth) &&
																f.LookupParameter("Flange Thickness").AsDouble() == Util.MmToFoot(Beam.FlangeTh) &&
																f.LookupParameter("Web Thickness").AsDouble() == Util.MmToFoot(Beam.WebTh)
																)
															{
																return true;
															}
															return false;
														});

			if (fs != null) return fs;

			Family family = default;
			try
			{
				fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.First(f => f.FamilyName == familyName);
			}
			catch (Exception)
			{
				string directory = $"C:/ProgramData/Autodesk/RVT {RevitVersion}/Libraries/US Imperial/Structural Framing/Steel/AISC 14.1/";
				family = OpenFamily(doc, directory, familyName);
				fs = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
			}

			ElementType fs1 = fs.Duplicate(String.Format("I-Beam Custom {0:0}x{1:0}", Beam.Width / 10, Beam.Depth / 10));
			fs1.LookupParameter("Width").Set(Util.MmToFoot(Beam.Width));
			fs1.LookupParameter("Height").Set(Util.MmToFoot(Beam.Depth));
			fs1.LookupParameter("Flange Thickness").Set(Util.MmToFoot(Beam.FlangeTh));
			fs1.LookupParameter("Web Thickness").Set(Util.MmToFoot(Beam.WebTh));
			fs1.LookupParameter("Web Fillet").Set(Util.MmToFoot(0.85 * (Beam.WebTh)));

			return fs1 as FamilySymbol;
		}

		public FamilySymbol GetLShapeBraceFamilySymbol(Document doc, Brace Brace, string familyName, BuiltInCategory category)
		{
			FamilySymbol fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.Where(f => f.FamilyName == familyName)
														.Where(f =>
														{
															if (f.LookupParameter("d").AsDouble() == Util.MmToFoot(Brace.Depth) &&
																f.LookupParameter("b").AsDouble() == Util.MmToFoot(Brace.Width) &&
																f.LookupParameter("t").AsDouble() == Util.MmToFoot(Brace.Thickness)
																) return true;

															return false;
														}).FirstOrDefault();

			if (fs != null) return fs;

			Family family = default;
			try
			{
				fs = new FilteredElementCollector(doc).OfCategory(category)
														.WhereElementIsElementType()
														.Cast<FamilySymbol>()
														.First(f => f.FamilyName == familyName);
			}
			catch (Exception)
			{
				string directory = $"C:/ProgramData/Autodesk/RVT {RevitVersion}/Libraries/US Imperial/Structural Columns/Steel/";
				family = OpenFamily(doc, directory, familyName);
				fs = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
			}

			ElementType fs1 = fs.Duplicate(String.Format($"L-Angle Custom {0:0}x{1:0}", Brace.Width / 10, Brace.Depth / 10));
			fs1.LookupParameter("b").Set(Util.MmToFoot(Brace.Width));
			fs1.LookupParameter("d").Set(Util.MmToFoot(Brace.Depth));
			fs1.LookupParameter("t").Set(Util.MmToFoot(Brace.Thickness));
			fs1.LookupParameter("k").Set(Util.MmToFoot(Brace.Thickness));
			fs1.LookupParameter("x").Set(Util.MmToFoot(0.28 * (Brace.Thickness)));
			fs1.LookupParameter("y").Set(Util.MmToFoot(0.28 * (Brace.Thickness)));

			return fs1 as FamilySymbol;
		}


		#endregion

		#region Steel Columns

		public FamilyInstance CreateSteelColumnInstance(Document doc, XYZ location, Level bottomLvl, Level topLvl, FamilySymbol colType, double rotationAngleRad, bool isStructural)
		{

			FamilyInstance col;
			StructuralType structuralType = isStructural ? StructuralType.Column : StructuralType.NonStructural;
			if (!colType.IsActive) colType.Activate();
			col = doc.Create.NewFamilyInstance(location, colType, bottomLvl, structuralType);
			col.LookupParameter("Top Level").Set(topLvl.Id);
			col.LookupParameter("Top Offset").Set(0);
			col.LookupParameter("Base Offset").Set(0);
			ElementTransformUtils.RotateElement(doc, col.Id, Line.CreateBound(location, location + XYZ.BasisZ), rotationAngleRad);


			return col;
		}

		public FamilyInstance CreateSteelColumnInstance(Document doc, ColumnSt column, FamilySymbol colType, bool isStructural, List<double> storeys)
		{

			XYZ location = new XYZ(Util.MmToFoot(column.Location.X), Util.MmToFoot(column.Location.Y), 0);
			Level colBottomLevel = GetLevel(doc, MapToStorey(column.BottomLevel, storeys));
			Level colTopLevel = GetLevel(doc, MapToStorey(column.TopLevel, storeys));

			double colRotation = column.RefDirection.X >= 0.0 ?
				90 + Vector3D.AngleBetween(new Vector3D(column.RefDirection.X, column.RefDirection.Y, column.RefDirection.Z), new Vector3D(0, -1, 0)) :
				90 + Vector3D.AngleBetween(new Vector3D(column.RefDirection.X, column.RefDirection.Y, column.RefDirection.Z), new Vector3D(0, -1, 0));

			colRotation *= Math.PI / 180.0;

			return CreateSteelColumnInstance(doc, location, colBottomLevel, colTopLevel, colType, colRotation, isStructural);

		}

		public List<FamilyInstance> CreateSteelColumnInstance(Document doc, List<ColumnSt> cols, string familyName, bool isStructural, List<double> storeys)
		{
			FamilySymbol colType;
			return cols.Select(col =>
			{
				colType = GetIShapeColumnFamilySymbol(doc, col, familyName, BuiltInCategory.OST_StructuralColumns);
				return CreateSteelColumnInstance(doc, col, colType, isStructural, storeys);

			}).ToList();
		}

		#endregion

		#region Steel Braces

		public FamilyInstance CreateBraceInstance(Document doc, XYZ location, XYZ endLocation, Level bottomLvl, Level topLvl, FamilySymbol colType, double rotationAngleRad, bool isStructural)
		{

			FamilyInstance Brace;
			StructuralType structuralType = isStructural ? StructuralType.Column : StructuralType.NonStructural;
			if (!colType.IsActive) colType.Activate();
			Line L1 = Line.CreateBound(location, endLocation);
			Brace = doc.Create.NewFamilyInstance(L1, colType, bottomLvl, structuralType);
			Brace.LookupParameter("Top Level").Set(topLvl.Id);
			Brace.LookupParameter("Top Offset").Set(0);
			Brace.LookupParameter("Base Offset").Set(0);
			// ElementTransformUtils.RotateElement(doc, Brace.Id, Line.CreateBound(location, endLocation), rotationAngleRad);


			return Brace;
		}

		public FamilyInstance CreateBraceInstance(Document doc, Brace Brace, FamilySymbol colType, bool isStructural, List<double> storeys)
		{
			XYZ location = new XYZ(Util.MmToFoot(Brace.Location.X), Util.MmToFoot(Brace.Location.Y), Util.MmToFoot(Brace.Location.Z));
			XYZ endLocation = location + Util.MmToFoot(Brace.Length) * new XYZ(Brace.Axis.X, Brace.Axis.Y, Brace.Axis.Z);

			Level colBottomLevel = GetLevel(doc, MapToStorey(Brace.BottomLevel, storeys));
			double toplevel = Util.FootToMm(endLocation.Z);
			Level colTopLevel = GetLevel(doc, MapToStorey(toplevel, storeys));

			double colRotation = Brace.RefDirection.X >= 0.0 ?
				90 + Vector3D.AngleBetween(new Vector3D(Brace.RefDirection.X, Brace.RefDirection.Y, Brace.RefDirection.Z), new Vector3D(0, -1, 0)) :
				90 + Vector3D.AngleBetween(new Vector3D(Brace.RefDirection.X, Brace.RefDirection.Y, Brace.RefDirection.Z), new Vector3D(0, -1, 0));

			colRotation *= Math.PI / 180.0;

			return CreateBraceInstance(doc, location, endLocation, colBottomLevel, colTopLevel, colType, colRotation, isStructural);
		}

		public List<FamilyInstance> CreateBraceInstance(Document doc, List<Brace> Braces, string familyName, bool isStructural, List<double> storeys)
		{
			FamilySymbol braceType;
			return Braces.Select(Brace =>
			{
				braceType = GetLShapeBraceFamilySymbol(doc, Brace, familyName, BuiltInCategory.OST_StructuralColumns);
				return CreateBraceInstance(doc, Brace, braceType, isStructural, storeys);

			}).ToList();
		}

		#endregion

		#region Steel Beams

		public FamilyInstance CreateSteelBeamInstance(Document doc, BeamSt beam, FamilySymbol fs)
		{
			StructuralType stBeam = StructuralType.Beam;
			XYZ p1 = new XYZ(Util.MmToFoot(beam.Location.X), Util.MmToFoot(beam.Location.Y), Util.MmToFoot(beam.Location.Z));
			XYZ p2 = p1 + beam.Length * new XYZ(Util.MmToFoot(beam.Axis.X), Util.MmToFoot(beam.Axis.Y), Util.MmToFoot(beam.Axis.Z));
			Line l1 = Line.CreateBound(p1, p2);
			FamilyInstance fi = doc.Create.NewFamilyInstance(l1, fs, null, stBeam);
			fi.LookupParameter("Reference Level").Set(GetLevel(doc, beam.Location.Z).Id);
			return fi;
		}
		public List<FamilyInstance> CreateSteelBeamInstance(Document doc, List<BeamSt> beamsSt, string familyName)
		{
			return beamsSt.Select(x => CreateSteelBeamInstance(doc, x, GetIShapeBeamFamilySymbol(doc, x, familyName, BuiltInCategory.OST_StructuralFraming))).ToList();
		}

		#endregion


	}
}
