using Autodesk.Revit.DB;
using HcBimUtils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DuNV_DATN.Core
{
	public class SectionColumn
	{
		public static void NewSection(Document document, Element element,int vitri)
		{
			XYZ max = new XYZ();
			XYZ min = new XYZ();
			var p=new List<XYZ>();
			var origin=(element.Location as LocationPoint).Point;
			var leght= element.GetParameter(BuiltInParameter.INSTANCE_LENGTH_PARAM).AsDouble();
			var height = document.GetElement(element.GetTypeId()).GetParameter("h").AsDouble();
			var width = document.GetElement(element.GetTypeId()).GetParameter("b").AsDouble();
			var dirr = (element as FamilyInstance).FacingOrientation;
			switch (vitri)
			{
				case 0:
					p = GetBou(origin, height, width, 100.MmToFoot(),dirr);
					break;
				case 1:
					p = GetBou(origin, height, width, leght/2, dirr);
					break;
				case 2:
					p = GetBou(origin, height, width, leght-100.MmToFoot(), dirr);
					break;
			}	
			var bbox = new BoundingBoxXYZ();
			bbox.Enabled = true;
			
			bbox.Max = p.FirstOrDefault();
			bbox.Min = p.LastOrDefault();
			var tran = Transform.Identity;
			tran.Origin = (bbox.Max+bbox.Min)/2;
			tran.BasisX = dirr.Normalize();
			tran.BasisY = XYZ.BasisZ;
			tran.BasisZ = dirr.CrossProduct(XYZ.BasisZ);
			bbox.Transform = tran;
			ViewFamilyType vft
				= new FilteredElementCollector(document)
				.OfClass(typeof(ViewFamilyType))
				.Cast<ViewFamilyType>()
				.FirstOrDefault<ViewFamilyType>(y =>
				ViewFamily.Section == y.ViewFamily);
			var viewSection = ViewSection.CreateSection(document, vft.Id, bbox);
		}
		private static List<XYZ> GetBou(XYZ ori,double height,double width,double ele,XYZ dir)
		{
			var result=new List<XYZ>();
			var dirX = dir.CrossProduct(XYZ.BasisZ);
			var p1=ori.Add(height*2*dir).Add(width*2*dirX);
			var p2=ori.Add(height*2*-dir).Add(width*2*-dirX);
			result.Add(p1.Add(ele*XYZ.BasisZ));
			result.Add(p2.Add(ele * XYZ.BasisZ));
			return result;
		}
	}
}
