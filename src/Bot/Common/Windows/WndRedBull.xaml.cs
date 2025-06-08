using Bot.Common;
using Bot.Common.Windows;
using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bot.Common.Windows
{
    public partial class WndRedBull : EtWindow
    {
		public WndRedBull()
		{
			InitializeComponent();
		}

		public static void MyShow()
		{
				ShowOneInstance(()=> { return new WndRedBull(); });
		}

	}
}
