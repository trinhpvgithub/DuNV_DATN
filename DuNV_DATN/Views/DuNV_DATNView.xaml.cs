using DuNV_DATN.ViewModels;

namespace DuNV_DATN.Views
{
	public partial class DuNV_DATNView
	{
		public DuNV_DATNView(DuNV_DATNViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}
}