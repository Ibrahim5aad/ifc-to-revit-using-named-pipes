using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using Xbim.Ifc4.Interfaces;

namespace IFCtoRevit.IFCLoader
{
    public static class Adapter
    {
        #region Columns

        public static Column GetColumnsData(IIfcColumn column)
        {

            Point location = new Point()
            {
                X = ((column.ObjectPlacement as IIfcLocalPlacement)
                    .RelativePlacement as IIfcAxis2Placement3D).Location.X,

                Y = ((column.ObjectPlacement as IIfcLocalPlacement)
                    .RelativePlacement as IIfcAxis2Placement3D).Location.Y,
                Z = 0

            };


            Point refDir = new Point()
            {
                X = ((column.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.X,

                Y = ((column.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.Y,

                Z = 0
            };

            Column col = new Column()
            {


                Location = location,

                Name = column.Name.Value.ToString(),

                BottomLevel = ((column.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Z,

                TopLevel = ((column.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Z + ((column.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth,

                Width = (((column.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcRectangleProfileDef).XDim,

                Depth = (((column.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcRectangleProfileDef).YDim,

                RefDirection = refDir


            };
            return col;
        }

        public static List<Column> GetColumnsData(List<IIfcColumn> columns)
        {

            return columns.Select(col => GetColumnsData(col)).ToList();
        }


        #endregion

        #region Levels

        public static List<double> GetStoreyLevels(List<IIfcBuildingStorey> storeys)
        {
            List<double> lvls = new List<double>();
            storeys.ForEach(st => lvls.Add(st.Elevation.Value));
            return lvls;
        }

        #endregion

        #region Floors

        public static FloorSlab GetFloorsData(IIfcSlab floor)
        {

            Point location = new Point()
            {
                X = ((floor.ObjectPlacement as IIfcLocalPlacement)
                    .RelativePlacement as IIfcAxis2Placement3D).Location.X,

                Y = ((floor.ObjectPlacement as IIfcLocalPlacement)
                    .RelativePlacement as IIfcAxis2Placement3D).Location.Y,
                Z = 0

            };

            Point refDir = new Point()
            {
                X = ((floor.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.X,

                Y = ((floor.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.Y,

                Z = 0
            };

            Vector3D refd = new Vector3D(refDir.X, refDir.Y, refDir.Z);
            Vector3D yaxis = Vector3D.CrossProduct(new Vector3D(0, 0, 1), refd);
            yaxis.Normalize();

            Matrix3D transform = new Matrix3D(
                                              refDir.X, refDir.Y, 0, 0,
                                              yaxis.X, yaxis.Y, 0, 0,
                                              0, 0, 1.0, 0,
                                              location.X, location.Y, location.Z, 1.0);

            FloorSlab f = new FloorSlab()
            {
                Name = floor.Name.Value.ToString(),

                Location = location,

                RefDirection = refDir,

                Mat = transform,

                Level = (floor.ContainedInStructure.FirstOrDefault().RelatingStructure as IIfcBuildingStorey).Elevation.Value,

                Profile = (((floor.Representation.Representations.FirstOrDefault().Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                                                        .SweptArea as IIfcArbitraryClosedProfileDef).OuterCurve as IIfcPolyline)
                                                        .Points.Select(pt => new Point(){ X = pt.X, Y = pt.Y, Z = 0 }).ToList(),

                Depth = (floor.Representation.Representations.FirstOrDefault().Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth,


            };
            return f;
        }

        public static List<FloorSlab> GetFloorsData(List<IIfcSlab> floors)
        {
            return floors.Select(floor => GetFloorsData(floor)).ToList();
        }


        #endregion

        #region Beams

        public static Beam GetBeamsData(IIfcBeam beam)
        {

            Point location = new Point();
            location.X = ((beam.ObjectPlacement as IIfcLocalPlacement)
            .RelativePlacement as IIfcAxis2Placement3D).Location.X;

            location.Y = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Location.Y;

            location.Z = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Location.Z;


            Point refDirection = new Point();

            refDirection.X = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).RefDirection.X;

            refDirection.Y = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).RefDirection.Y;

            refDirection.Z = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).RefDirection.Z;

            Point axis = new Point();

            axis.X = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Axis.X;

            axis.Y = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Axis.Y;

            axis.Z = ((beam.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Axis.Z;

            Beam B = new Beam()
            {
                Name = beam.Name,
                RefDirection = refDirection,
                Axis = axis,
                Location = location,
                H = (((beam.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcRectangleProfileDef).YDim,
                B = (((beam.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcRectangleProfileDef).XDim,
                Length = ((beam.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth
            };

            return B;
        }
        public static List<Beam> GetBeamsData(List<IIfcBeam> beam)
        {
            List<Beam> myBeam = new List<Beam>();
            foreach (IIfcBeam x in beam)
            {
                myBeam.Add(GetBeamsData(x));
            }
            return myBeam;
        }


        #endregion

        #region Slanted Columns

        public static Inclined GetInclinesData(IIfcMemberStandardCase inclined)
        {

            Point location = new Point();
            Point refDir = new Point();
            Point axis = new Point();

            location.X = ((inclined.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.X;

            location.Y = ((inclined.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Y;

            location.Z = ((inclined.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Z;


            refDir.X = ((inclined.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.X;

            refDir.Y = ((inclined.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.Y;

            refDir.Z = ((inclined.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.Z;


            axis.X = ((inclined.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).Axis.X;

            axis.Y = ((inclined.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).Axis.Y;

            axis.Z = ((inclined.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).Axis.Z;

            Inclined inc = new Inclined()
            {


                Location = location,
                RefDirection = refDir,
                Axis = axis,

                Name = inclined.Name.Value.ToString(),

                Length = ((inclined.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth,

                BottomLevel = location.Z,

                //TopLevel = location.Z+ ((inclined.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth*cos(angleBetween(axis,z-unitVec)),
                //Calculated in creation at revitplugin

                Width = (((inclined.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcRectangleProfileDef).XDim,

                Depth = (((inclined.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcRectangleProfileDef).YDim,
            };
            return inc;
        }

        public static List<Inclined> GetInclinesData(List<IIfcMemberStandardCase> inclines)
        {
            return inclines.Select(inclined => GetInclinesData(inclined)).ToList();
        }


        #endregion

        #region Steel Columns

        public static ColumnSt GetColumnsSteelData(IIfcColumn columnSt)
        {

            Point location = new Point();

            location.X = ((columnSt.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.X;

            location.Y = ((columnSt.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Y;
            location.Z = 0;

            Point refDir = new Point();

            refDir.X = ((columnSt.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.X;

            refDir.Y = ((columnSt.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.Y;

            refDir.Z = 0;

            ColumnSt col = new ColumnSt()
            {

                RefDirection = refDir,

                Location = location,

                Name = columnSt.Name.Value.ToString(),

                BottomLevel = ((columnSt.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Z,

                TopLevel = ((columnSt.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Z + ((columnSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth,

                Width = (((columnSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).OverallWidth,

                Depth = (((columnSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).OverallDepth,

                FlangeTh = (((columnSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).FlangeThickness,

                WebTh = (((columnSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).WebThickness,

            };
            return col;
        }
        public static List<ColumnSt> GetColumnsSteelData(List<IIfcColumn> columnsSt)
        {
            return columnsSt.Select(col => GetColumnsSteelData(col)).ToList();
        }


        #endregion

        #region Steel Beams

        public static BeamSt GetBeamsSteelData(IIfcBeam beamSt)
        {

            Point location = new Point();
            location.X = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
            .RelativePlacement as IIfcAxis2Placement3D).Location.X;

            location.Y = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Location.Y;

            location.Z = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Location.Z;


            Point refDirection = new Point();

            refDirection.X = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).RefDirection.X;

            refDirection.Y = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).RefDirection.Y;

            refDirection.Z = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).RefDirection.Z;

            Point axis = new Point();

            axis.X = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Axis.X;

            axis.Y = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Axis.Y;

            axis.Z = ((beamSt.ObjectPlacement as IIfcLocalPlacement)
                          .RelativePlacement as IIfcAxis2Placement3D).Axis.Z;

            BeamSt B = new BeamSt()
            {
                Name = beamSt.Name,
                RefDirection = refDirection,
                Axis = axis,
                Location = location,
                Length = ((beamSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth,

                Width = (((beamSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).OverallWidth,

                Depth = (((beamSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).OverallDepth,

                FlangeTh = (((beamSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).FlangeThickness,

                WebTh = (((beamSt.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcIShapeProfileDef).WebThickness,
            };

            return B;
        }

        public static List<BeamSt> GetBeamsSteelData(List<IIfcBeam> beamSt)
        {
            List<BeamSt> myBeam = new List<BeamSt>();
            foreach (IIfcBeam x in beamSt)
            {
                myBeam.Add(GetBeamsSteelData(x));
            }
            return myBeam;
        }


        #endregion

        #region Steel Braces

        public static Brace GetBracesData(IIfcMemberStandardCase brace)
        {

            Point location = new Point();
            Point refDir = new Point();
            Point axis = new Point();

            location.X = ((brace.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.X;

            location.Y = ((brace.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Y;

            location.Z = ((brace.ObjectPlacement as IIfcLocalPlacement)
                        .RelativePlacement as IIfcAxis2Placement3D).Location.Z;


            refDir.X = ((brace.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.X;

            refDir.Y = ((brace.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.Y;

            refDir.Z = ((brace.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).RefDirection.Z;


            axis.X = ((brace.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).Axis.X;

            axis.Y = ((brace.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).Axis.Y;

            axis.Z = ((brace.ObjectPlacement as IIfcLocalPlacement).RelativePlacement as IIfcAxis2Placement3D).Axis.Z;

            Brace brace1 = new Brace()
            {
                Location = location,
                RefDirection = refDir,
                Axis = axis,

                Name = brace.Name.Value.ToString(),

                Length = ((brace.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth,

                BottomLevel = location.Z,

                //TopLevel = location.Z+ ((inclined.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid).Depth*cos(angleBetween(axis,z-unitVec)),
                //Calculated in creation at revitplugin

                Width = (double)(((brace.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcLShapeProfileDef).Width,

                Depth = (((brace.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcLShapeProfileDef).Depth,

                Thickness = (((brace.Representation.Representations.FirstOrDefault() as IIfcShapeRepresentation).Items.FirstOrDefault() as IIfcExtrudedAreaSolid)
                        .SweptArea as IIfcLShapeProfileDef).Thickness,
            };
            return brace1;
        }

        public static List<Brace> GetBracesData(List<IIfcMemberStandardCase> braces)
        {
            return braces.Select(brace => GetBracesData(brace)).ToList();
        }


        #endregion
    }
}
