using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HcBimUtils;
using HcBimUtils.DocumentUtils;
using HcBimUtils.GeometryUtils;
using HcBimUtils.RebarUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DuNV_DATN.Core
{
	public static class AutoTag
	{
		public static XYZ FindVecXYPerp(this XYZ fv)
		{
			if (fv.IsPerpendicular(XYZ.BasisX))
			{
				return XYZ.BasisX;
			}
			else if (fv.IsPerpendicular(XYZ.BasisY))
			{
				return XYZ.BasisY;
			}
			else
			{
				return new XYZ(fv.Y, fv.X, fv.Z);
			}
			//return null;
		}
		public static FamilySymbol DocTypeTuFamily(Document doc, string FamilyName, string TypeName)
		{
			FilteredElementCollector symColl = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
			var targetElems = symColl.Where(x => x.Name.Equals(TypeName));
			FamilySymbol type = null;

			foreach (var item in targetElems)
			{
				type = item as FamilySymbol;
				if (type.Name == FamilyName)
				{
					break;
				}
			}
			return type;
		}
		public static void CreateTag(Document document,View view,Element column)
		{
			var tran=new Transaction(document,"aa");
			tran.Start();
			FamilySymbol T_TagThepDon = DocTypeTuFamily(document, "NhanThep", "SO LUONG");
			FamilySymbol T_TagThepDai = DocTypeTuFamily(document, "NhanThep", "KHOANG CACH");
			TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
			var rebar = new FilteredElementCollector(document, view.Id)
				.OfCategory(BuiltInCategory.OST_Rebar)
				.WhereElementIsNotElementType()
				.ToList();
			//IndependentTag tag=IndependentTag.Create(document,tagMode,AC.ActiveView.Id,new Reference(column),false,TagOrientation.Horizontal,(column.Location as LocationPoint).Point);
			rebar.ForEach(x =>
			{
				IList<ElementId> lstId = new List<ElementId>();
				var ps = GetRebarTag(x as Rebar,column);
				MultiReferenceAnnotationType TypeRef;
				FilteredElementCollector famSym = new FilteredElementCollector(document).OfClass(typeof(MultiReferenceAnnotationType));
				TypeRef = famSym.ToElements().First() as MultiReferenceAnnotationType;
				lstId.Add((x as Rebar).Id);
				MultiReferenceAnnotationOptions Options = new MultiReferenceAnnotationOptions(TypeRef);
				Options.TagHeadPosition = ps.Last();
				Options.DimensionLineOrigin = ps.FirstOrDefault();
				Options.DimensionLineDirection = XYZ.BasisY;
				Options.DimensionPlaneNormal = view.ViewDirection;
				Options.SetElementsToDimension(lstId);
			MultiReferenceAnnotation mar = MultiReferenceAnnotation.Create(document, view.Id, Options);
			document.GetElement(mar.GetTypeId()).get_Parameter(BuiltInParameter.MULTI_REFERENCE_ANNOTATION_TAG_TYPE).Set(T_TagThepDon.Id);

			IndependentTag tag = document.GetElement(mar.TagId) as IndependentTag;
				//IndependentTag tag = IndependentTag.Create(document, view.Id, new Reference(x), true, tagMode, TagOrientation.Horizontal,GetRebarLocation(x as Rebar));
				//tag.LeaderEndCondition = LeaderEndCondition.Free;
			});
			tran.Commit();


		}
		public static void CreateHorTag(Document document, View view, Element column)
		{
			FamilySymbol T_TagThepDon = DocTypeTuFamily(document, "NhanThep", "SO LUONG");
			FamilySymbol T_TagThepDai = DocTypeTuFamily(document, "NhanThep", "KHOANG CACH");
			var tran = new Transaction(document, "aa");
			tran.Start();
			//TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
			var rebar = new FilteredElementCollector(document, view.Id)
				.OfCategory(BuiltInCategory.OST_Rebar)
				.WhereElementIsNotElementType().Where(x => (x as Rebar).GetHostId() == column.Id)
				.ToList();
			double bdam = document.GetElement(column.GetTypeId()).GetParameters("b")[0].AsDouble();

			rebar.ForEach(x =>
			{
				//var ps = GetRebarLocation(x as Rebar);
				var ps = GetRebarTag(x as Rebar, column);

				if ((x as Rebar).LayoutRule != RebarLayoutRule.MaximumSpacing)
				{

					MultiReferenceAnnotationType TypeRef;
					FilteredElementCollector famSym = new FilteredElementCollector(document).OfClass(typeof(MultiReferenceAnnotationType));

					TypeRef = famSym.ToElements().First() as MultiReferenceAnnotationType;

					IList<ElementId> lstId = new List<ElementId>();
					lstId.Add((x as Rebar).Id);
					MultiReferenceAnnotationOptions Options = new MultiReferenceAnnotationOptions(TypeRef);
					Options.TagHeadPosition = new XYZ(column.GetColumnLocationPoint().X + bdam / 2 + 1, ps.FirstOrDefault().Y, ps.FirstOrDefault().Z);
					Options.DimensionLineOrigin = ps.FirstOrDefault();
					Options.DimensionLineDirection = XYZ.BasisX;
					Options.DimensionPlaneNormal = view.ViewDirection;
					Options.SetElementsToDimension(lstId);


					MultiReferenceAnnotation mar = MultiReferenceAnnotation.Create(document, view.Id, Options);
					document.GetElement(mar.GetTypeId()).get_Parameter(BuiltInParameter.MULTI_REFERENCE_ANNOTATION_TAG_TYPE).Set(T_TagThepDon.Id);

					IndependentTag tag = document.GetElement(mar.TagId) as IndependentTag;
					//(tag.Location as LocationPoint).Point.Movetag(column, document);

				}
				else
				{
					IndependentTag tag = IndependentTag.Create(document, T_TagThepDai.Id, view.Id, new Reference(document.GetElement((x as Rebar).Id)), true, TagOrientation.Horizontal, new XYZ(column.GetColumnLocationPoint().X + bdam / 2 + 1, column.GetColumnLocationPoint().Y - 0.5, ps.FirstOrDefault().Z));
					tag.Move(XYZ.BasisX * 0.25);

					//IndependentTag tag = IndependentTag.Create(document, view.Id, new Reference(x), true, tagMode, TagOrientation.Horizontal, GetRebarLocation(x as Rebar));
					//tag.LeaderEndCondition = LeaderEndCondition.Free;
					//tag.SetLeaderElbow(ps.FirstOrDefault());
					//tag.TagHeadPosition = ps.FirstOrDefault();
				}
			});

			tran.Commit();
		}
		public static void CreateVerTag(Document document, View view, Element column)
		{
			FamilySymbol T_TagThepDon = DocTypeTuFamily(document, "NhanThep", "SO LUONG");
			FamilySymbol T_TagThepDai = DocTypeTuFamily(document, "NhanThep", "KHOANG CACH");
			var tran = new Transaction(document, "aa");
			tran.Start();
			//TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
			var rebar = new FilteredElementCollector(document, view.Id)
				.OfCategory(BuiltInCategory.OST_Rebar)
				.WhereElementIsNotElementType().Where(x => (x as Rebar).GetHostId() == column.Id)
				.ToList();
			double bdam = document.GetElement(column.GetTypeId()).GetParameters("b")[0].AsDouble();
			double hdam = document.GetElement(column.GetTypeId()).GetParameters("h")[0].AsDouble();

			double i = 1;
			rebar.ForEach(x =>
			{


				if ((x as Rebar).LayoutRule == RebarLayoutRule.MaximumSpacing)
				{
					IndependentTag tag = IndependentTag.Create(document, T_TagThepDai.Id, view.Id, new Reference(document.GetElement((x as Rebar).Id)), false, TagOrientation.Vertical, new XYZ(0, column.GetColumnLocationPoint().Y - 5, column.GetColumnHeight() / 2 - 1.5));
					//tag.Move(XYZ.BasisX * 0.25);			

				}
				else
				{
					IndependentTag tag = IndependentTag.Create(document, T_TagThepDon.Id, view.Id, new Reference(document.GetElement((x as Rebar).Id)), true, TagOrientation.Horizontal, new XYZ(0, 0, 0));
					tag.TagHeadPosition = new XYZ(0, column.GetColumnLocationPoint().Y + hdam + 3, column.GetColumnLocationPoint().Z + i);
					if (view.IsElementVisibleInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate, column.Id))
					{
						view.HideElementTemporary(tag.Id);
					}
				}
				i += 1.5;
			});

			tran.Commit();
		}
		private static XYZ GetRebarLocation(Rebar rebar)
		{
			XYZ result= new XYZ();
			IList<Curve> curveRb = rebar.GetShapeDrivenAccessor().ComputeDrivingCurves();
			var r= curveRb.FirstOrDefault();
			result = (r.GetEndPoint(0) + r.GetEndPoint(1)) / 2;
			return result;
		}
		private static List<XYZ> GetRebarTag(Rebar rebar,Element column)
		{
			List<XYZ> result = new List<XYZ>();
			var point = new XYZ(0, GetRebarLocation(rebar).Y, 0);
			var p = new XYZ(0, column.GetColumnLocationPoint().Y, 0);
			var dir = p - point;
			ICollection<ElementId> re = new Collection<ElementId>();

			//UIDocument uidoc = AC.UiDoc;
			//re.Add(rebar.Id);
			//uidoc.Selection.SetElementIds(re);
			//TaskDialog.Show("da", rebar.Name + " " + dir.Normalize().ToString());
			var p1 = point;

			if (dir.Normalize().IsSameDirection(XYZ.BasisY))
			{
				p1 = point.Add(XYZ.BasisY * -0.35);
			}
			else if (dir.Normalize().IsSameDirection(-XYZ.BasisY))
			{
				p1 = point.Add(XYZ.BasisY * 0.35);

			}
			var p2 = point.Add(dir * 4000.MmToFoot());
			result.Add(p1);
			result.Add(p2);
			return result;
		}
		private static XYZ GetPoint(Element coll,XYZ point)
		{
			var col = coll as FamilyInstance;
			var faces=col.GetFaces();
			//var face = col.GetFaces().OrderBy(x => x.GetPoints().FirstOrDefault().Z).FirstOrDefault(x => (x as PlanarFace).FaceNormal.DotProduct(XYZ.BasisZ).Equals(0));
			List<Face> faces1 = new List<Face>();
            foreach (var item in faces)
            {
                if((item as PlanarFace).FaceNormal.DotProduct(XYZ.BasisZ).Equals(0))
				{
					faces1.Add(item);
				}
            }
			var ps= new List<XYZ>();
			faces1.ForEach(x =>
			{
				ps.Add(GetProjectPoint(x as PlanarFace,point));
			});
			return ps.MinBy2(x => (x - point).GetLength());
        }
		public static double GetSignedDistance(PlanarFace plane, XYZ point)
		{
			var v = point - plane.Origin;
			return Math.Abs(GeometryUtils.DotMatrix(plane.FaceNormal, v));
		}
		public static bool IsPointInPlane(PlanarFace plane, XYZ point)
		{
			return GeometryUtils.IsEqual(GetSignedDistance(plane, point), 0);
		}
		public static XYZ GetProjectPoint(PlanarFace plane, XYZ point)
		{
			var d = GetSignedDistance(plane, point);
			var q = GeometryUtils.AddXYZ(point, GeometryUtils.MultiplyVector(plane.FaceNormal, d));
			return IsPointInPlane(plane, q) ? q : GeometryUtils.AddXYZ(point, GeometryUtils.MultiplyVector(plane.FaceNormal, -d));
		}
	}
}
