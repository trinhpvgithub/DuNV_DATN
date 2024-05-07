using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HcBimUtils;
using HcBimUtils.DocumentUtils;
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
		public static void CreateTag(Document document,View view)
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
				IndependentTag tag = IndependentTag.Create(document, view.Id, new Reference(x), true, tagMode, TagOrientation.Vertical,GetRebarLocation(x as Rebar));
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
			var lines = GetLines(column, AC.Document);
			var l= lines.MinBy2(x=>point.DistancePoint2Line(x));
			return result;	
		}
		private static List<Line> GetLines(Element coll, Document doc)
		{
			var col = coll as FamilyInstance;
			var oriPoint = (col.Location as LocationPoint).Point;
			var type = doc.GetElement(col.GetTypeId()) as ElementType;
			var w = type.LookupParameter("b").AsDouble();
			var l = type.LookupParameter("h").AsDouble();
			var facing = col.FacingOrientation.Normalize();
			var hand = col.HandOrientation.Normalize();
			var p1 = oriPoint.Add(-hand * w / 2 + -facing * l / 2);
			var p2 = oriPoint.Add(hand * w / 2 + -facing * l / 2);
			var p3 = p2.Add(facing * l);
			var p4 = p1.Add(facing * l);
			List<XYZ> xYZs = new List<XYZ>() { p1, p2, p3, p4 };
			var Lines = new List<Line>();
			Lines.Add(p1.CreateLine(p2));
			Lines.Add(p2.CreateLine(p3));
			Lines.Add(p3.CreateLine(p4));
			Lines.Add(p4.CreateLine(p1));
			return Lines;
		}
	}
}
