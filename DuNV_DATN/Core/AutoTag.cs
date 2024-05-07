using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HcBimUtils;
using HcBimUtils.DocumentUtils;
using HcBimUtils.GeometryUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DuNV_DATN.Core
{
	public class AutoTag
	{
		public static void CreateTag(Document document,View view,Element column)
		{
			var tran=new Transaction(document,"aa");
			tran.Start();
			TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
			var rebar = new FilteredElementCollector(document, view.Id)
				.OfCategory(BuiltInCategory.OST_Rebar)
				.WhereElementIsNotElementType()
				.ToList();
			//IndependentTag tag=IndependentTag.Create(document,tagMode,AC.ActiveView.Id,new Reference(column),false,TagOrientation.Horizontal,(column.Location as LocationPoint).Point);
			rebar.ForEach(x =>
			{
				var ps = GetRebarTag(x as Rebar,column);
				IndependentTag tag = IndependentTag.Create(document, view.Id, new Reference(x), true, tagMode, TagOrientation.Horizontal,GetRebarLocation(x as Rebar));
				tag.LeaderEndCondition = LeaderEndCondition.Free;
				tag.LeaderElbow = ps.FirstOrDefault();
				tag.TagHeadPosition = ps.LastOrDefault();
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
			List<XYZ> result= new List<XYZ>();
			var point= GetRebarLocation(rebar);
			var p = GetPoint(column, point);
			var dir =p-point;
			var p1 = point.Add(dir * 3000.MmToFoot());
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
