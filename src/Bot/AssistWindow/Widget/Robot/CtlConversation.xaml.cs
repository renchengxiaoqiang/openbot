using Bot.Automation.ChatDeskNs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bot.AssistWindow.Widget.Robot
{
    /// <summary>
    /// Interaction logic for CtlDialog.xaml
    /// </summary>
    public partial class CtlConversation : UserControl
    {
        private string sendedChar = "   √√";
        public CtlConversation()
        {
            InitializeComponent();
        }


        public static CtlConversation Create(string question, string answer,bool isAutoReply = false)
        {
            var dlg = new CtlConversation();
            dlg.Setup(question,answer, isAutoReply);
            return dlg;
        }

        public void Setup(string question,string answer,bool isAutoReply)
        {
            txtQuestion.Text = question;
            txtAnswer.Text = answer + (isAutoReply ? sendedChar : "");
        }

        private void txtAnswer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            if (!tb.Text.EndsWith(sendedChar))
            {
                tb.Text += sendedChar;
            }
        }
    }
}
