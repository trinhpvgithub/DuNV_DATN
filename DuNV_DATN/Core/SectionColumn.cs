using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuNV_DATN.Core
{
	public class SectionColumn
	{
		public static void NewSection(Document document, Element element)
		{
			var ebox = element.get_BoundingBox(null);
			//laay ra size cua bou
			var w = (ebox.Max.X - ebox.Min.X);
			var d = (ebox.Max.Y - ebox.Min.Y);
			var h = (ebox.Max.Z - ebox.Min.Z);
			//lam cho no de nhin hon
			if (w < 10) w = 10;
			if (d < 10) d = 10;
			var maxPt = new XYZ(w, h, 0);
			var minPt = new XYZ(-w, -h, -d);
			var bbox = new BoundingBoxXYZ();
			bbox.Enabled = true;
			bbox.Max = maxPt;
			bbox.Min = minPt;
			var tran = Transform.Identity;
			var midPt = 0.5 * (ebox.Max - ebox.Min);
			tran.Origin = midPt;
			tran.BasisX = XYZ.BasisX;
			tran.BasisY = XYZ.BasisY;
			tran.BasisZ = XYZ.BasisZ;
			bbox.Transform = tran;
			ViewFamilyType vft
				= new FilteredElementCollector(document)
				.OfClass(typeof(ViewFamilyType))
				.Cast<ViewFamilyType>()
				.FirstOrDefault<ViewFamilyType>(y =>
				ViewFamily.Section == y.ViewFamily);
			var viewSection = ViewSection.CreateSection(document, vft.Id, bbox);
		}
	}
}
