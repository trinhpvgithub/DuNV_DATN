using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
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
	}
}
