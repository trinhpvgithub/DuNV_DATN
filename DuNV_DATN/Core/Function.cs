using Autodesk.Revit.DB;
using HcBimUtils.DocumentUtils;
using HcBimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuNV_DATN.Core
{
	public class Function
	{
		public static void Dim(View View,string scale, Document doc)
		{
			var Grids = new FilteredElementCollector(doc)
			   .OfClass(typeof(Grid))
			   .Cast<Grid>()
			   .ToList();
			if (Grids.Count < 1)
			{
				IO.ShowWarning("Model của bạn đang thiếu các đường lưới trục!");
				return;
			}
			var GridX = Grids.FirstOrDefault();
			List<Grid> Grid = new List<Grid>();
			List<Grid> GridY = new List<Grid>();
			Grids.ForEach(x =>
			{
				if (x.Curve.Direction().IsParallel(GridX.Curve.Direction()))
				{
					Grid.Add(x);
				}
				else GridY.Add(x);
			});
			var a1 = new ReferenceArray();
			var a2 = new ReferenceArray();
			var a3 = new ReferenceArray();
			var a4 = new ReferenceArray();
			Grid.ForEach(x =>
			{
				a1.Append(new Reference(x));
			});
			GridY.ForEach(x =>
			{
				a2.Append(new Reference(x));
			});
			a3.Append(new Reference(Grid.FirstOrDefault()));
			a3.Append(new Reference(Grid.LastOrDefault()));
			a4.Append(new Reference(GridY.FirstOrDefault()));
			a4.Append(new Reference(GridY.LastOrDefault()));
			var line = offsetLine(Grid.LastOrDefault().Curve.GetEndPoint(0).CreateLine(Grid.FirstOrDefault().Curve.GetEndPoint(0)), Grid.FirstOrDefault().Curve.Direction(),scale);
			var line1 = offsetLine(GridY.LastOrDefault().Curve.GetEndPoint(0).CreateLine(GridY.FirstOrDefault().Curve.GetEndPoint(0)), GridY.FirstOrDefault().Curve.Direction(), scale);
			var line2 = offsetLine(line, Grid.FirstOrDefault().Curve.Direction(),scale, 2);
			var line3 = offsetLine(line1, GridY.FirstOrDefault().Curve.Direction(),scale, 2);
			var tran = new Transaction(doc);
			tran.Start("dim");
			doc.Create.NewDimension(View, line, a3);
			doc.Create.NewDimension(View, line1, a4);
			doc.Create.NewDimension(View, line2, a1);
			doc.Create.NewDimension(View, line3, a2);
			DimColumn(View,scale,doc);
			tran.Commit();
		}
		private static int ViewScale(string s)
		{
			string[] chuoi = null;
			chuoi = s.Split(':');
			return int.Parse(chuoi.LastOrDefault());
		}
		public static Line offsetLine(Line line, XYZ dir,string scale1, double a = 1)
		{
			var scale = ViewScale(scale1);
			var f = line.GetEndPoint(0);
			var l = line.GetEndPoint(1);
			var ff = f.Add(a * 8.MmToFoot() * scale * dir);
			var ll = l.Add(a * 8.MmToFoot() * scale * dir);
			return ff.CreateLine(ll);
		}
		public static void DimColumn(View View,string scale,Document doc)
		{
			var columns = new FilteredElementCollector(doc, View.Id)
			   .WhereElementIsNotElementType()
			   .OfCategory(BuiltInCategory.OST_StructuralColumns)
			   .ToList();
			var Grids = new FilteredElementCollector(doc)
			   .OfClass(typeof(Grid))
			   .Cast<Grid>()
			   .ToList();
			var Grid = Grids.FirstOrDefault();
			List<Grid> GridX = new List<Grid>();
			List<Grid> GridY = new List<Grid>();
			Grids.ForEach(x =>
			{
				if (x.Curve.Direction().IsParallel(Grid.Curve.Direction()))
				{
					GridX.Add(x);
				}
				else GridY.Add(x);
			});
			columns.ForEach(c =>
			{
				var Lines = GetPointCol(c,doc);
				var GridnearX = GridX.MinBy2(g => g.Curve.Distance((c.Location as LocationPoint).Point));
				var GridnearY = GridY.MinBy2(g => g.Curve.Distance((c.Location as LocationPoint).Point));
				var x = new Reference(GridnearX);
				var y = new Reference(GridnearY);
				if (Lines.FirstOrDefault().IsParallelTo(GridnearX.Curve as Line))
				{
					var line = Lines.FirstOrDefault();
					var p1 = line.GetEndPoint(0);
					var p2 = line.GetEndPoint(1);
					var normal = line.Direction.CrossProduct(XYZ.BasisZ);
					line = offsetLine(line, normal,scale);
					var detailLine1 = doc.Create.NewDetailCurve(View, Line.CreateBound(p1, p1 + normal * 1.MmToFoot()));
					var detailLine2 = doc.Create.NewDetailCurve(View, Line.CreateBound(p2, p2 + normal * 1.MmToFoot()));
					ReferenceArray references = new ReferenceArray();
					references.Append(new Reference(detailLine1));
					references.Append(new Reference(GridnearY));
					references.Append(new Reference(detailLine2));

					// create the new dimension
					Dimension dimension = doc.Create.NewDimension(View,
																		line, references);
					var line1 = Lines.LastOrDefault();
					var p3 = line1.GetEndPoint(0);
					var p4 = line1.GetEndPoint(1);
					var normal1 = line1.Direction.CrossProduct(XYZ.BasisZ);
					line1 = offsetLine(line1, -normal1, scale);
					var detailLine3 = doc.Create.NewDetailCurve(View, Line.CreateBound(p3, p3 + normal1 * 1.MmToFoot()));
					var detailLine4 = doc.Create.NewDetailCurve(View, Line.CreateBound(p4, p4 + normal1 * 1.MmToFoot()));
					ReferenceArray references1 = new ReferenceArray();
					references1.Append(new Reference(detailLine3));
					references1.Append(new Reference(GridnearX));
					references1.Append(new Reference(detailLine4));

					// create the new dimension
					Dimension dimension1 = doc.Create.NewDimension(View,
																		line1, references1);

				}
				else
				{
					var line = Lines.FirstOrDefault();
					var p1 = line.GetEndPoint(0);
					var p2 = line.GetEndPoint(1);
					var normal = line.Direction.CrossProduct(XYZ.BasisZ);
					line = offsetLine(line, normal,scale);
					var detailLine1 = doc.Create.NewDetailCurve(View, Line.CreateBound(p1, p1 + normal * 1.MmToFoot()));
					var detailLine2 = doc.Create.NewDetailCurve(View, Line.CreateBound(p2, p2 + normal * 1.MmToFoot()));
					ReferenceArray references = new ReferenceArray();
					references.Append(new Reference(detailLine1));
					references.Append(new Reference(GridnearX));
					references.Append(new Reference(detailLine2));

					// create the new dimension
					Dimension dimension = doc.Create.NewDimension(View,
																		line, references);
					var line1 = Lines.LastOrDefault();
					var p3 = line1.GetEndPoint(0);
					var p4 = line1.GetEndPoint(1);
					var normal1 = line1.Direction.CrossProduct(XYZ.BasisZ);
					line1 = offsetLine(line1, -normal1, scale);
					var detailLine3 = doc.Create.NewDetailCurve(View, Line.CreateBound(p3, p3 + normal1 * 1.MmToFoot()));
					var detailLine4 = doc.Create.NewDetailCurve(View, Line.CreateBound(p4, p4 + normal1 * 1.MmToFoot()));
					ReferenceArray references1 = new ReferenceArray();
					references1.Append(new Reference(detailLine3));
					references1.Append(new Reference(GridnearY));
					references1.Append(new Reference(detailLine4));

					// create the new dimension
					Dimension dimension1 = doc.Create.NewDimension(View,
																		line1, references1);

				}
			});
		}
		public static List<Line> GetPointCol(Element coll,Document doc)
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
			Lines.Add(p1.CreateLine(p4));
			return Lines;
		}
	}
}
