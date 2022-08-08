using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace IFCtoRevit
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	class JoinElements : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiapp = commandData.Application;
			UIDocument uidoc = uiapp.ActiveUIDocument;
			//Application app = uiapp.Application;
			Document doc = uidoc.Document;

			// get all walls on the active view
			FilteredElementCollector floors =
			  new FilteredElementCollector(doc, doc.ActiveView.Id);
			floors.OfClass(typeof(Floor));

			foreach (Floor w in floors)
			{
				// get columns on the active view
				FilteredElementCollector beams = new FilteredElementCollector(doc, doc.ActiveView.Id)
					.OfClass(typeof(FamilyInstance))
					.OfCategory(BuiltInCategory.OST_StructuralFraming);

				// as we don't want all columns, let's filter
				// by the wall bounding box (intersect)
				BoundingBoxXYZ bb = w.get_BoundingBox(doc.ActiveView);
				Outline outline = new Outline(bb.Min, bb.Max);
				BoundingBoxIntersectsFilter bbfilter =
				  new BoundingBoxIntersectsFilter(outline);

				beams.WherePasses(bbfilter);


				using (Transaction t = new Transaction(doc, "Join Elements"))
				{
					t.Start();
					foreach (FamilyInstance b in beams)
					{
						JoinGeometryUtils.JoinGeometry(doc, w, b);
					}
					t.Commit();
				}
			}
			return Result.Succeeded;
		}
	}
}
