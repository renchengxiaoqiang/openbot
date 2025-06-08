using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using Bot.Common;
using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Extensions;
using BotLib.Misc;
using DbEntity;

namespace Bot.AssistWindow.Widget.Robot
{
    public partial class CtlOneGoods : UserControl
    {
        public ZnkfItem GoodsInfo;

        public CtlOneGoods(ZnkfItem gi)
        {
            InitializeComponent();
            InitUI(gi);
        }

        private void InitUI(ZnkfItem gi)
        {
            GoodsInfo = gi;
            imgGoods.Init(gi.pic, gi.itemUrl);
            tblkTitle.Text = gi.title;
            tblkTitle.ToolTip = "双击，发送链接\r\n\r\n右击，弹出菜单";
            tblkTitle.MouseDown += tblkTitle_MouseDown;
            tblkTitle.ContextMenu = (ContextMenu)base.FindResource("menuGoodsTitle");

            tblkOuterId.MouseDown += grdRight_MouseDown;
            grdRight.MouseDown += grdRight_MouseDown;

            tblkPrice.Text = gi.price;
            tblkNum.Text = gi.quantity + "件";
        }

        private void grdRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                ClipboardEx.SetTextSafe(tblkOuterId.Text);
            }
        }

        private void tblkTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                //_desk.Editor.SendPlainText(GoodsInfo.BasicInfo.GoodsUrl);
            }
        }

        private void btnOpenInShop_Click(object sender, RoutedEventArgs e)
        {
            //var url = "https://item.publish.taobao.com/sell/publish.htm?itemId=" + GoodsInfo.BasicInfo.NumIid;
            //_desk.NavWithWorkbenchAsync(url);
        }

        private void btnCopyGoodsUrl_Click(object sender, RoutedEventArgs e)
        {
            ClipboardEx.SetTextSafe(GoodsInfo.itemUrl);
        }


        private void btnOpenInBrowers_Click(object sender, RoutedEventArgs e)
        {
            Util.Nav(GoodsInfo.itemUrl);
        }

        private void btnCopyGoodsNumiid_Click(object sender, RoutedEventArgs e)
        {
            var txt = string.Empty;
            if (GoodsInfo != null)
            {
                txt = string.Format("{0}({1}),", GoodsInfo.itemId.ToString(), GoodsInfo.title);
            }
            ClipboardEx.SetTextSafe(txt);
        }


        private void btnCopyGoodsTitle_Click(object sender, RoutedEventArgs e)
        {
            ClipboardEx.SetTextSafe(GoodsInfo.title, false, 2, true, true);
        }

        private void btnCopyOuterId_Click(object sender, RoutedEventArgs e)
        {
            ClipboardEx.SetTextSafe(tblkOuterId.Text);
        }

    }
}
