using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using DuNV_DATN.Core;
using DuNV_DATN.Views;
using HcBimUtils.DocumentUtils;
using HcBimUtils.WPFUtils;
using System.Windows;

namespace DuNV_DATN.ViewModels
{
	public class DuNV_DATNViewModel : ViewModelBase
	{
		#region khai báo
		public DuNV_DATNView MainView { get; set; }
		public string ColumnName { get; set; }
		private int _section;
		public int SeclectSection
		{
			get { return _section; }
			set
			{
				_section = value;
				OnPropertyChanged();
			}
		}
		public List<int> Section { get; set; }
		public List<FamilySymbol> TitleBlocks { get; set; }
		private FamilySymbol _selectedTitleBlock;
		public FamilySymbol SelectedTitleBlock
		{
			get => _selectedTitleBlock;
			set
			{
				_selectedTitleBlock = value;
				OnPropertyChanged();
			}
		}
		public List<View> ViewTemplate { get; set; }
		private View _selectedViewTemplate;
		public View SelectedViewTemplate
		{
			get => _selectedViewTemplate;
			set
			{
				_selectedViewTemplate = value;
				OnPropertyChanged();
			}
		}
		public List<string> Scale { get; set; }
		private string _selectedScale;
		public string SelectedScale
		{
			get => _selectedScale;
			set
			{
				_selectedScale = value;
				OnPropertyChanged();
			}
		}
		public Element Column {  get; set; }
		public List<View> Viewplan { get; set; }
		public RelayCommand Ok { get; set; }
		public RelayCommand Cancel { get; set; }
		public RelayCommand PickColumn { get; set; }
		#endregion
		#region core
		public DuNV_DATNViewModel(DuNV_DATNView v)
		{
			MainView = v;
			MainView.DataContext = this;
			GetData();
			PickColumn = new RelayCommand(x => Pick());
		}
		public void GetData()
		{
			TitleBlocks = new FilteredElementCollector(AC.Document)
				.WhereElementIsElementType()
				.OfCategory(BuiltInCategory.OST_TitleBlocks)
				.Cast<FamilySymbol>()
				.ToList();

			SelectedTitleBlock = TitleBlocks.FirstOrDefault();
			ViewTemplate = new FilteredElementCollector(AC.Document)
			   .OfClass(typeof(View))
			   .Cast<View>()
			   .Where(x => x.IsTemplate).ToList();
			SelectedViewTemplate = ViewTemplate.FirstOrDefault();
			Scale = new List<string> { "1:50", "1:100", "1:150", "1:200" };
			SelectedScale = Scale.FirstOrDefault();
			Section = new List<int> { 2,3 };
			SeclectSection = Section.FirstOrDefault();
		}
		public void Pick()
		{
			var columns = new FilteredElementCollector(AC.Document)
				.WhereElementIsNotElementType()
				.OfCategory(BuiltInCategory.OST_StructuralColumns)
				.Cast<Element>()
				.ToList();
			if(columns.Count>0)
			{
				MainView.Hide();
				Column = Utils.PickColumn();
				ColumnName=Column.Name;
				OnPropertyChanged(nameof(ColumnName));
				MainView.Show();
			}	
			else
			{
				MessageBox.Show("Model đang không có cái cột nào", "Warning");
			}	
		}
		#endregion
	}
}