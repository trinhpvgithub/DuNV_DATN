
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HcBimUtils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuNV_DATN.Core
{
	public static class SectionColumn
	{
		public static Element NewSection(Document document, Element element, int vitri, int scale)
		{
			XYZ max = new XYZ();
			XYZ min = new XYZ();
			var p = new List<XYZ>();
			var origin = (element.Location as LocationPoint).Point;
			var leght = element.GetParameter(BuiltInParameter.INSTANCE_LENGTH_PARAM).AsDouble();
			var height = document.GetElement(element.GetTypeId()).GetParameter("h").AsDouble()/2;
			var width = document.GetElement(element.GetTypeId()).GetParameter("b").AsDouble()/2;
			var dirr = (element as FamilyInstance).FacingOrientation;
			switch (vitri)
			{
				case 0:
					p = GetBou(origin, height, width, 100.MmToFoot(), dirr);
					break;
				case 1:
					p = GetBou(origin, height, width, leght / 2, dirr);
					break;
				case 2:
					p = GetBou(origin, height, width, leght - 100.MmToFoot(), dirr);
					break;
			}
			var bbox = new BoundingBoxXYZ();
			bbox.Enabled = true;

			bbox.Max = p.FirstOrDefault();
			bbox.Min = p.LastOrDefault();
			var tran = Transform.Identity;
			//tran.Origin = (bbox.Max+bbox.Min)/2;
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
			var aa = ViewSection.CreateSection(document, vft.Id, bbox);
			aa.Scale = scale;
			aa.CropBoxVisible = false;
			aa.get_Parameter(BuiltInParameter
			  .VIEW_DETAIL_LEVEL).Set(3);

			aa.get_Parameter(BuiltInParameter
			  .MODEL_GRAPHICS_STYLE).Set(2);
			var eles = new FilteredElementCollector(document, aa.Id)
.WhereElementIsNotElementType()
.Where(x =>
{
	var type = x.GetTypeId();
	var ele = document.GetElement(type);
	if (ele is ViewFamilyType vft)
	{
		return vft.ViewFamily == ViewFamily.Section;
	}
	return false;
});
			if (eles.Count() > 0) aa.HideElements(eles.Select(x => x.Id).ToList());
			OverrideGraphicSettings ogs = new OverrideGraphicSettings();
			ogs.SetCutForegroundPatternId(new FilteredElementCollector(document).WherePasses(new ElementClassFilter(typeof(FillPatternElement))).
ToElements().Cast<FillPatternElement>().Where(x => x.GetFillPattern().IsSolidFill == true).First().Id);
			ogs.SetCutForegroundPatternColor(new Color((byte)255, (byte)255, (byte)255));
			aa.SetElementOverrides(element.Id, ogs);//màu cột

			var rebar = new FilteredElementCollector(document, aa.Id)
			.OfCategory(BuiltInCategory.OST_Rebar)
			.WhereElementIsNotElementType().Where(x => (x as Rebar).GetHostId() == element.Id)
			.ToList();


			rebar.ForEach(x =>
			{
				if ((x as Rebar).LayoutRule != RebarLayoutRule.MaximumSpacing)
				{
					ogs.SetCutLineColor(new Color((byte)255, (byte)0, (byte)0));
					aa.SetElementOverrides(x.Id, ogs);//màu thép đơn
				}
				else
				{
					ogs.SetSurfaceForegroundPatternId(new FilteredElementCollector(document).WherePasses(new ElementClassFilter(typeof(FillPatternElement))).
ToElements().Cast<FillPatternElement>().Where(x => x.GetFillPattern().IsSolidFill == true).First().Id);
					ogs.SetSurfaceForegroundPatternColor(new Color((byte)0, (byte)255, (byte)0));
					aa.SetElementOverrides(x.Id, ogs);//màu thép đai
				}
			});

			return aa;
		}
		private static List<XYZ> GetBou(XYZ ori, double height, double width, double ele, XYZ dir)
		{
			var result = new List<XYZ>();
			var dirX = dir.CrossProduct(XYZ.BasisZ);
			var p1 = ori.Add(height * 2 * dir).Add(width * 2 * dirX);
			var p2 = ori.Add(height * 2 * -dir).Add(width * 2 * -dirX);
			//var p1 = new XYZ(w,h,0);
			//var p2=new XYZ(-w,-h,-d);
			result.Add(p1.Add((ele + 500.MmToFoot()) * XYZ.BasisZ));
			result.Add(p2.Add(ele * XYZ.BasisZ));
			return result;
		}
		public static Element NewSectionDoc(Document doc, Element e, int scale, bool showOnlyReb)
		{
			XYZ loco = e.GetColumnLocationPoint();
			FamilyInstance fi = e as FamilyInstance;
			Line line = fi.GetSweptProfile().GetDrivingCurve() as Line;

			ViewFamilyType vft
	= new FilteredElementCollector(doc)
	  .OfClass(typeof(ViewFamilyType))
	  .Cast<ViewFamilyType>()
	  .FirstOrDefault<ViewFamilyType>(y =>
	   ViewFamily.Section == y.ViewFamily);

			var rebars = new FilteredElementCollector(doc, doc.ActiveView.Id)
		   .OfCategory(BuiltInCategory.OST_Rebar)
		   .WhereElementIsNotElementType().Where(x => (x as Rebar).GetHostId() == e.Id)
		   .ToList();

			XYZ p = line.GetEndPoint(0);
			XYZ q = line.GetEndPoint(1);
			XYZ v = q - p;

			BoundingBoxXYZ bb = e.get_BoundingBox(null);

			double w = v.GetLength();
			double h = bb.Max.Y - bb.Min.Y;
			double d = bb.Max.X - bb.Min.X;

			XYZ min = new XYZ();
			XYZ max = new XYZ();


			min = new XYZ(bb.Min.Y, -0.5, 0);
			max = new XYZ(bb.Max.Y + h + 5, rebars.GetMaxLengthRebs() + 0.5, d / 2);

			//TaskDialog.Show("dasd", bb.Max.Y.ToString() + "\n" + bb.Min.Y.ToString() + "\n" + h.ToString() + "\n\n" + bb.Max.X.ToString() + "\n" + bb.Min.X.ToString() + "\n" + d.ToString() + "\n\n" + min.X.ToString() + "\n" + max.X.ToString() + "\n" + e.GetColumnLocationPoint());
			Transform t = Transform.Identity;
			t.Origin = loco.Add(XYZ.BasisY * -(min.X + h + 5));
			t.BasisX = XYZ.BasisY;
			t.BasisY = XYZ.BasisZ;
			t.BasisZ = XYZ.BasisX;

			BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
			sectionBox.Transform = t;
			sectionBox.Min = min;
			sectionBox.Max = max;

			ViewSection view = ViewSection.CreateSection(doc, vft.Id, sectionBox);

			view.get_Parameter(BuiltInParameter
			 .VIEW_DETAIL_LEVEL).Set(3);

			view.get_Parameter(BuiltInParameter
			  .MODEL_GRAPHICS_STYLE).Set(2);
			view.CropBoxVisible = false;
			view.Scale = scale;
			var eles = new FilteredElementCollector(doc, view.Id)
.WhereElementIsNotElementType()
.Where(x =>
{
	var type = x.GetTypeId();
	var ele = doc.GetElement(type);
	if (ele is ViewFamilyType vft)
	{
		return vft.ViewFamily == ViewFamily.Section;
	}
	return false;
});
			if (eles.Count() > 0) view.HideElements(eles.Select(x => x.Id).ToList());

			OverrideGraphicSettings ogs = new OverrideGraphicSettings();
			ogs.SetCutForegroundPatternId(new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(FillPatternElement))).
ToElements().Cast<FillPatternElement>().Where(x => x.GetFillPattern().IsSolidFill == true).First().Id);
			ogs.SetCutForegroundPatternColor(new Color((byte)255, (byte)255, (byte)255));
			view.SetElementOverrides(e.Id, ogs);//màu cột

			var rebar = new FilteredElementCollector(doc, view.Id)
			.OfCategory(BuiltInCategory.OST_Rebar)
			.WhereElementIsNotElementType().Where(x => (x as Rebar).GetHostId() == e.Id)
			.ToList();

			//TaskDialog.Show("dá", rebar.GetMaxLengthRebs().FootToMm().ToString());
			List<Element> rebDoc = new List<Element>();


			rebar.ForEach(x =>
			{

				if ((x as Rebar).LayoutRule == RebarLayoutRule.MaximumSpacing)
				{
					ogs.SetSurfaceForegroundPatternId(new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(FillPatternElement))).
ToElements().Cast<FillPatternElement>().Where(x => x.GetFillPattern().IsSolidFill == true).First().Id);
					ogs.SetSurfaceForegroundPatternColor(new Color((byte)0, (byte)255, (byte)0));
					ogs.SetCutLineColor(new Color((byte)0, (byte)255, (byte)0));
					view.SetElementOverrides(x.Id, ogs);//màu thép đai

					if (showOnlyReb == true)
					{
						view.HideElementTemporary(x.Id);
						view.HideElementTemporary(e.Id);

					}

				}
				else
				{
					ogs.SetSurfaceForegroundPatternId(new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(FillPatternElement))).
ToElements().Cast<FillPatternElement>().Where(x => x.GetFillPattern().IsSolidFill == true).First().Id);
					ogs.SetSurfaceForegroundPatternColor(new Color((byte)255, (byte)0, (byte)0));
					view.SetElementOverrides(x.Id, ogs);//màu thép đown
					if (showOnlyReb == true)
					{
						rebDoc.Add(x);
					}
				}
			});
			//TaskDialog.Show("dasd", w.ToString() + "\n"+ h.ToString() + "\n"+ d.ToString() + "\n"+ min.ToString() + "\n" +max.ToString() + "\n\n" + sectionBox.Min.ToString() + "\n" + sectionBox.Max.ToString() + "\n\n" + view.Origin.ToString() + "\n" + loco+"\n\n" + (view.Origin.Y-loco.Y));
			if (rebDoc.Count > 0)
			{
				List<double> kichthuocthep = new List<double>();
				rebDoc.ForEach(x =>
				{
					Rebar re = x as Rebar;
					if (!kichthuocthep.Contains(re.TotalLength / re.Quantity))
					{
						kichthuocthep.Add(re.TotalLength / re.Quantity);
					}

				});
				RebarCoverType rct = doc.getRebarCover(e.LookupParameter("Rebar Cover - Bottom Face").AsElementId());
				double LopBv = rct.CoverDistance;

				kichthuocthep.Sort();
				kichthuocthep.CreateDimReb(doc, view, loco.Add(XYZ.BasisY * -doc.GetElement(e.GetTypeId()).GetParameter("h").AsDouble()), LopBv);
			}

			return view;
		}
		public static RebarCoverType getRebarCover(this Autodesk.Revit.DB.Document doc, ElementId id)
		{
			//hinh dang thep
			FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(RebarCoverType));
			IList<Element> shapeElems = fec.ToElements();
			foreach (var shapeElem in shapeElems)
			{
				RebarCoverType shape = shapeElem as RebarCoverType;
				if (shape.Id == id)
					return shape;
			}
			return null;
		}
		public static void CreateDimReb(this List<double> ktReb, Document doc, ViewSection view, XYZ vitri, double lopBV)
		{

			double i = 1;
			ktReb.ForEach(x =>
			{
				ReferenceArray referenceArray = new ReferenceArray();
				//TaskDialog.Show("das", x.FootToMm().ToString());
				var ln1 = doc.Create.NewDetailCurve(view, Line.CreateBound(vitri.Add(XYZ.BasisY * -(i)).Add(XYZ.BasisZ * lopBV), vitri.Add(XYZ.BasisY * -(0.25 + i)).Add(XYZ.BasisZ * lopBV)));
				var ln2 = doc.Create.NewDetailCurve(view, Line.CreateBound(vitri.Add(XYZ.BasisY * -(i)).Add(XYZ.BasisZ * (x + lopBV)), vitri.Add(XYZ.BasisY * -(0.25 + i)).Add(XYZ.BasisZ * (x + lopBV))));
				referenceArray.Append(ln1.GeometryCurve.Reference);
				referenceArray.Append(ln2.GeometryCurve.Reference);
				doc.Create.NewDimension(view, Line.CreateBound(vitri.Add(XYZ.BasisY * -(0.25 + i)).Add(XYZ.BasisZ * lopBV), vitri.Add(XYZ.BasisY * -(0.25 + i)).Add(XYZ.BasisZ * (x + lopBV))), referenceArray);
				i += 2;
			});
		}
		public static double GetMaxLengthRebs(this List<Element> rebs)
		{
			double maxLength = 0;

			rebs.ForEach(x =>
			{
				Rebar re = x as Rebar;
				if (maxLength < (re.TotalLength / re.Quantity))
				{
					maxLength = (re.TotalLength / re.Quantity);
				}
			});

			return maxLength;
		}
	}
}
