using Autodesk.Revit.Attributes;
using DuNV_DATN.ViewModels;
using DuNV_DATN.Views;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;

namespace DuNV_DATN.Commands
{
	[UsedImplicitly]
	[Transaction(TransactionMode.Manual)]
	public class Command : ExternalCommand
	{
		public override void Execute()
		{
			AC.GetInformation(UiDocument);
			var view = new DuNV_DATNView();
			var viewModel = new DuNV_DATNViewModel(view);
			view.ShowDialog();
		}
	}
}