using BotLib.Wpf.Extensions;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bot.Common.Windows
{
    public partial class WndMsgBox : EtWindow
    {
        private Action<bool> _callback;

        private bool _isYesButtonClicked;

        public WndMsgBox(string message, string title, bool showCancelButton, Action<bool> callback = null)
		{
			_isYesButtonClicked = false;
			InitializeComponent();
			tblContent.Inlines.AddRange(InlineEx.ConvertTextToInlineConsiderUrl(message));
			if (!string.IsNullOrEmpty(title))
			{
				Title = title;
			}
			if (!showCancelButton)
			{
				btnCancel.Visibility = Visibility.Collapsed;
			}
			_callback = callback;
			Closed += new EventHandler(WndMsgBox_Closed);
		}

        private void WndMsgBox_Closed(object sender, EventArgs e)
        {
            if (_callback != null)
            {
                _callback(_isYesButtonClicked);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _isYesButtonClicked = false;
            if (WindowEx.xIsModal(this))
            {
                DialogResult = false;
            }
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            _isYesButtonClicked = true;
            if (WindowEx.xIsModal(this))
            {
                DialogResult = true;
            }
            Close();
        }
    }
}
