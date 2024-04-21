using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuNV_DATN.Core
{
	public class IO
	{
		public static void ShowInfo(string content, string title = "Info")
		{
			MessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		public static void ShowWarning(string content, string title = "Warning")
		{
			MessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
	}
}
