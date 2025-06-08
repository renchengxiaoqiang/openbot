using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using Bot.Automation.ChatDeskNs;
using DbEntity;
using BotLib.Db.Sqlite;
using BotLib.Wpf.Extensions;
using System.Security.Cryptography;
using Bot.ChromeNs;
using Bot.AssistWindow.Widget.Robot;
using System.Collections.Concurrent;
using System.Linq;
using OpenAI.Chat;
using BotLib.Extensions;
using SuperSocket.SocketEngine.Configuration;
using Top.Api.Domain;

namespace Bot.AssistWindow.Widget.Robot
{
    public partial class CtlRobot : UserControl
    {
        private Desk _desk;
        private RightPanel _rightPanel;
        private WndAssist _wndDontUse;
        private QN _preQN;
        private ConcurrentDictionary<string, List<CtlConversation>> buyerConversations;

        public CtlRobot(Desk desk, RightPanel rp)
        {
            InitializeComponent();
            _desk = desk;
            _rightPanel = rp;
            buyerConversations = new ConcurrentDictionary<string, List<CtlConversation>>();
            Loaded += CtlRobot_Loaded;
        }

        private WndAssist Wnd
        {
            get
            {
                if (_wndDontUse == null)
                {
                    _wndDontUse = (this.xFindParentWindow() as WndAssist);
                }
                return _wndDontUse;
            }
            set
            {
                _wndDontUse = value;
            }
        }

        private void CtlRobot_Loaded(object sender, RoutedEventArgs e)
        {
            cboxAuto.IsChecked = Params.Robot.GetIsAutoReply();
        }

        public void AddConversation(string seller, string buyer, string question, string answer,bool isAutoReply = false)
        {
            var key = string.Format("{0}#{1}", seller, buyer);
            var ctlConversation = CtlConversation.Create(question, answer, isAutoReply);
            var conversations = buyerConversations.xTryGetValue(key);
            if (conversations == null || conversations.Count < 1)
            {
                conversations = new List<CtlConversation>() { ctlConversation };
            }
            else
            {
                conversations.Add(ctlConversation);
            }

            buyerConversations.AddOrUpdate(key, id => conversations, (k, v) => conversations);

            if (QN.CurQN.Seller.Nick == seller && QN.CurQN.Buyer.Nick == buyer)
            {
                grdTipNoConv.Visibility = Visibility.Collapsed;
                stkDialog.Children.Add(ctlConversation);
            }
            scvBody.ScrollToEnd();
        }

        private void ShowGridTip(Grid gd)
        {
            grdTipNoGoods.xIsVisible(gd == grdTipNoGoods);
            grdDownGoods.xIsVisible(gd == grdDownGoods);
            grdTipNoConv.xIsVisible(gd == grdTipNoConv);
            grdShowConv.xIsVisible(gd == grdShowConv);
        }

        public async void ReShowAfterQNChange()
        {
            if (QN.CurQN != null)
            {
                _preQN = QN.CurQN;
                ShowGridTip(grdShowConv);
                ShowGridTip(grdDownGoods);
                //咨询的商品
                var itemRecord = await _preQN.GetItemRecords(_preQN.Buyer.TargetId);
                if (itemRecord.data == null || itemRecord.data.underInquiryItemList == null)
                {
                    //bdGoods.Child = grdTipNoGoods;

                    ShowGridTip(grdTipNoGoods);
                }
                else
                {
                    var inquiryItems = itemRecord.data.underInquiryItemList;
                    if (inquiryItems != null && inquiryItems.Count > 0)
                    {
                        var item = inquiryItems.First();
                        var ctlGoods = new CtlOneGoods(item);
                        //bdGoods.Child = ctlGoods;
                        stkGoods.Children.Add(ctlGoods);
                        grdTipNoGoods.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //bdGoods.Child = grdTipNoGoods;
                        ShowGridTip(grdTipNoGoods);
                    }
                }


                var key = string.Format("{0}#{1}", _preQN.Seller.Nick, _preQN.Buyer.Nick);
                var conversations = buyerConversations.xTryGetValue(key);
                stkDialog.Children.Clear();
                if (conversations != null && conversations.Count > 0)
                {
                    conversations.ForEach(conv => stkDialog.Children.Add(conv));
                    scvBody.ScrollToEnd();
                    grdTipNoConv.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowGridTip(grdTipNoConv);
                    stkDialog.Children.Add(grdTipNoConv);
                }
            }
        }

        public void ChangeBuyer(string buyer)
        {
            txtBuyer.Text = buyer;
            ReShowAfterQNChange();
        }

        private void cboxAuto_Click(object sender, RoutedEventArgs e)
        {
            Params.Robot.SetIsAutoReply(cboxAuto.IsChecked ?? false);
        }
    }
}
