using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using Bot.Automation.ChatDeskNs;
using DbEntity;
using BotLib.Db.Sqlite;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using Top.Api.Domain;
using Xceed.Wpf.Toolkit;
using Bot.ChromeNs;

namespace Bot.AssistWindow.Widget.Right.Goods
{
    public partial class CtlSearch : UserControl
    {
        public CtlSearch()
        {
            InitializeComponent();
        }
    }
}
