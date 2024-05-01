using Autodesk.Revit.DB;
using HcBimUtils;
using HcBimUtils.MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuNV_DATN.Core
{
	public class CreateSheet
	{
		public static void NewSheet(Document document, ElementId titleblock, List<Element> views, string name)
		{
			ViewSheet SHEET = null;
			Transaction trans = new Transaction(document);
			trans.Start("sheet");
			try
			{
				SHEET = ViewSheet.Create(document, titleblock);
				SHEET.Name = name;
				if (null == SHEET)
				{
					throw new Exception("Failed to create new ViewSheet.");
				}

				// Add passed in view onto the center of the sheet
				UV location = new UV((SHEET.Outline.Max.U - SHEET.Outline.Min.U) / 2,
										(SHEET.Outline.Max.V - SHEET.Outline.Min.V) / 2);
				var width = (SHEET.get_BoundingBox(null).Max.X - SHEET.get_BoundingBox(null).Min.X).MmToFoot();
				var height = (SHEET.get_BoundingBox(null).Max.Y - SHEET.get_BoundingBox(null).Min.Y).MmToFoot();

				if (views.Count==3)
				{
					Viewport.Create(document, SHEET.Id, views[0].Id, new XYZ(location.U, location.V+height, 0));
					Viewport.Create(document, SHEET.Id, views[1].Id, new XYZ(location.U, location.V, 0));
					Viewport.Create(document, SHEET.Id, views[2].Id, new XYZ(location.U, location.V-height, 0));
				}
				else
				{
					Viewport.Create(document, SHEET.Id, views[0].Id, new XYZ(location.U, location.V+height/3, 0));
					Viewport.Create(document, SHEET.Id, views[1].Id, new XYZ(location.U, location.V-height/3, 0));
				}
				//viewSheet.AddView(view3D, location);
				trans.Commit();
			}
			catch
			{
				trans.RollBack();
			}
		}
	}
}
