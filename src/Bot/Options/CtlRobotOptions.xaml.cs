using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Bot.AssistWindow;
using Bot.Common;
using Bot.Common.Windows;
using BotLib;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using BotLib.Extensions;
using DbEntity;
using static Bot.Params;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using Bot.Common.Trivial;
using Bot.Asset;
using Bot.Automation.ChatDeskNs;
using SuperSocket.Common;
using SuperSocket.SocketEngine.Configuration;

namespace Bot.Options
{
    public partial class CtlRobotOptions : UserControl, IOptions
    {
        private string _seller;
        private string _sellerMain;

        private string _sendImagePath;
        private SynableImageHelper _imageHelper = new SynableImageHelper(TransferFileTypeEnum.RuleAnswerImage);

        public CtlRobotOptions(string seller)
        {
            InitializeComponent();
            InitUI(seller);
        }

        public OptionEnum OptionType
        {
            get
            {
                return OptionEnum.RemindPay;
            }
        }


        public void InitUI(string seller)
        {
            _seller = seller;
            _sellerMain = TbNickHelper.GetMainPart(seller);
            txtBaseUrl.Text = Params.Robot.GetBaseUrl();
            txtApiKey.Text = Params.Robot.GetApiKey();
            txtModelName.Text = Params.Robot.GetModelName();
            txtSystemPrompt.Text = Params.Robot.GetSystemPrompt();

        }

        public void NavHelp()
        {
            throw new NotImplementedException();
        }

        public void RestoreDefault()
        {
            Params.Robot.SetBaseUrl(string.Empty);
            Params.Robot.SetApiKey(string.Empty);
            Params.Robot.SetModelName(string.Empty);
            Params.Robot.SetSystemPrompt(string.Empty);
        }

        public void Save(string seller)
        {
            Params.Robot.SetBaseUrl( txtBaseUrl.Text.Trim());
            Params.Robot.SetApiKey( txtApiKey.Text.Trim());
            Params.Robot.SetModelName(txtModelName.Text.Trim());
            Params.Robot.SetSystemPrompt(txtSystemPrompt.Text.Trim());
        }


        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
