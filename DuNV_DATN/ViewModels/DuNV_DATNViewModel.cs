﻿using Autodesk.Revit.DB;
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
		public Element Column { get; set; }
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
			Ok = new RelayCommand(x => ButtonOk());
			Cancel=new RelayCommand(x => ButtonCancel());
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
			Scale = new List<string> { "1:25", "1:50", "1:100", "1:150", "1:200" };
			SelectedScale = Scale.FirstOrDefault();
			Section = new List<int> { 2, 3 };
			SeclectSection = Section.FirstOrDefault();
		}
		public void Pick()
		{
			var columns = new FilteredElementCollector(AC.Document)
				.WhereElementIsNotElementType()
				.OfCategory(BuiltInCategory.OST_StructuralColumns)
				.Cast<Element>()
				.ToList();
			if (columns.Count > 0)
			{
				MainView.Hide();
				Column = Utils.PickColumn();
				ColumnName = Column.Name;
				OnPropertyChanged(nameof(ColumnName));
				MainView.ShowDialog();
			}
			else
			{
				IO.ShowWarning("Model đang không có cái cột nào");
			}
		}
		public void ButtonOk()
		{
			List<Element> elements = new List<Element>();
			using (Transaction ts=new Transaction(AC.Document,"aa"))
			{
				ts.Start();
				elements.Add(SectionColumn.NewSection(AC.Document, Column,1,ViewScale(SelectedScale)));
				elements.Add(SectionColumn.NewSection(AC.Document, Column,0, ViewScale(SelectedScale)));
				elements.Add(SectionColumn.NewSection(AC.Document, Column, 2, ViewScale(SelectedScale)));
				//SectionColumn.NewSectionDoc(AC.Document, Column, 3);
				ts.Commit();
			}
			elements.ForEach(element => { Function.Dim(element as View, SelectedScale, AC.Document); });
			elements.ForEach(x =>
			{
				AutoTag.CreateHorTag(AC.Document, x as View, Column);

			});
			List<Element> mcDoc = new List<Element>();
			using (Transaction ts = new Transaction(AC.Document, "aa"))
			{
				ts.Start();
				mcDoc.Add(SectionColumn.NewSectionDoc(AC.Document, Column,ViewScale(SelectedScale), false));
				mcDoc.Add(SectionColumn.NewSectionDoc(AC.Document, Column, ViewScale(SelectedScale), true));

				ts.Commit();
			}
			//elements.ForEach(element => { Function.Dim(element as View, SelectedScale, AC.Document); });
			mcDoc.ForEach(x =>
			{
				AutoTag.CreateVerTag(AC.Document, x as View, Column);
			});
			elements.AddRange(mcDoc);
			CreateSheet.NewSheet(AC.Document, SelectedTitleBlock.Id, elements, "MC-cot");
			MainView?.Close();
		}
		public void ShowView()
		{
			MainView.ShowDialog();
		}
		public void ButtonCancel()
		{
			MainView?.Close();
		}
		private int ViewScale(string s)
		{
			string[] chuoi = null;
			chuoi = s.Split(':');
			return int.Parse(chuoi.LastOrDefault());
		}
		#endregion
	}
}