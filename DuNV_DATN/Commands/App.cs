using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;

namespace DuNV_DATN.Commands
{
	[UsedImplicitly]
	public class App : ExternalApplication
	{
		public override void OnStartup()
		{
			CreateRibbon();
		}
		private void CreateRibbon()
		{
			var panel = Application.CreatePanel("Button", "NVD");
			var tools = panel.AddPushButton<Command>("CreateColumnSection");
			//tools.SetImage("/DuNV_DATN;component/Resources/Icons/column_16.png");
			//tools.SetLargeImage("/DuNV_DATN;component/Resources/Icons/column_32.png");
		}
	}
}
