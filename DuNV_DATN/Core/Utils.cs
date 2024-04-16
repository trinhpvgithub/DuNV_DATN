using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HcBimUtils.DocumentUtils;

namespace DuNV_DATN.Core
{
	/// <summary>
	///     The class contains wrapping methods for working with the Revit API.
	/// </summary>
	public static class Utils
	{
		public static Element PickColumn()
		{
			var ele= AC.UiDoc.Selection.PickObject(ObjectType.Element, new ColumnFilter()).ToElement().Cast<Element>();
			return ele;
		}
	}
	public class ColumnFilter : ISelectionFilter
	{
		protected readonly HashSet<Autodesk.Revit.DB.ElementId> mCategoryIds;

		public bool AllowElement(Autodesk.Revit.DB.Element elem)
		{
			if (elem.Category.ToBuiltinCategory() == Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns)
			{
				return true;
			}
			return false;
		}

		public bool AllowReference(Reference reference, Autodesk.Revit.DB.XYZ position)
		{
			return false;
		}

	}
}