using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using System.Windows.Media.Imaging;

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
			PushButton tools = panel.AddPushButton<Command>("CreateColumnSection");
			tools.Image = new BitmapImage(new Uri("/DuNV_DATN;component/Resources/column_16.png", UriKind.RelativeOrAbsolute));
			tools.LargeImage= new BitmapImage(new Uri("/DuNV_DATN;component/Resources/column_32.png", UriKind.RelativeOrAbsolute));
		}
	}
}
