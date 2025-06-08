using Bot.ChromeNs;
using DbEntity.Response;
using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bot.Automation.ChatDeskNs;
using Top.Api.Domain;
using Bot.ChatRecord;
using Newtonsoft.Json;
using BotLib;
using System.Diagnostics;
using System.Threading;
using Bot.AssistWindow.Widget.Robot;
using BotLib.Wpf.Extensions;
using OpenAI.Chat;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Bot.ChromeNs
{
    public class QN
    {
        public event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        public event EventHandler<SellerSwitchedEventArgs> EvSellerSwitched;
        public event EventHandler<MessageNotifyEventArgs> EvMessageNotity;
        public event EventHandler<RecieveNewMessageEventArgs> EvRecieveNewMessage;
        public event EventHandler<ShopRobotReceriveNewMessageEventArgs> EvShopRobotReceriveNewMessage;
        public static HashSet<QN> QNSet { get; set; }

        private CDPClient cdp;
        public CDPClient CDP
        {
            get
            {
                return cdp;
            }
            set
            {
                cdp = value;

                cdp.EvBuyerSwitched -= Cdp_EvBuyerSwitched;
                cdp.EvMessageNotity -= Cdp_EvMessageNotity;
                cdp.EvRecieveNewMessage -= Cdp_EvRecieveNewMessage;
                cdp.EvSellerSwitched -= Cdp_EvSellerSwitched;
                cdp.EvShopRobotReceriveNewMessage -= Cdp_EvShopRobotReceriveNewMessage;

                cdp.EvBuyerSwitched += Cdp_EvBuyerSwitched;
                cdp.EvMessageNotity += Cdp_EvMessageNotity;
                cdp.EvRecieveNewMessage += Cdp_EvRecieveNewMessage;
                cdp.EvSellerSwitched += Cdp_EvSellerSwitched;
                cdp.EvShopRobotReceriveNewMessage += Cdp_EvShopRobotReceriveNewMessage;
            }
        }

        private LocalUser _seller;
        public LocalUser Seller
        {
            get
            {
                return _seller;
            }
            set
            {
                _seller = value;
            }
        }

        public QNRpa rpa;
        public QNRpa Rpa { get { return rpa; } }
        public Conversation Buyer { get; set; }

        public static QN CurQN = null;

        static QN()
        {
            QNSet = new HashSet<QN>();
        }

        public QN(LocalUser seller)
        {
            this._seller = seller;
            this.rpa = new QNRpa(this);
        }


        public async void SendTextAsync(string buyer, string text)
        {
            await rpa.SendTextAsync(buyer, text);
        }


        public async void SendImageAsync(string buyer, string imagePath)
        {
            await rpa.SendImageAsync(buyer, imagePath);
        }

        private void Cdp_EvShopRobotReceriveNewMessage(object sender, ShopRobotReceriveNewMessageEventArgs e)
        {
            if (Params.Robot.GetIsAutoReply())
            {
                //打开买家
                OpenChat(e.Buyer.Nick);
            }
            if (EvShopRobotReceriveNewMessage != null)
            {
                EvShopRobotReceriveNewMessage(this, e);
            }
        }

        private void Cdp_EvSellerSwitched(object sender, SellerSwitchedEventArgs e)
        {
            Seller = e.Seller;
            Buyer = e.Buyer;
            CurQN = this;
            Desk.Inst.ChangeBuyer(e.Buyer.Nick);

            if (EvSellerSwitched != null)
            {
                EvSellerSwitched(this, e);
            }
        }

        private void Cdp_EvRecieveNewMessage(object sender, RecieveNewMessageEventArgs e)
        {
            if (EvRecieveNewMessage != null)
            {
                EvRecieveNewMessage(this, e);
            }

            var chatRes = JsonConvert.DeserializeObject<ChatResponse>(e.Message);
            var messages = chatRes.result;
            messages.ForEach(async m =>
            {
                if (m.fromid.nick != _seller.Nick && m.toid.nick == _seller.Nick)
                {
                    var isAutoReply = Params.Robot.GetIsAutoReply();
                    var answer = MyOpenAI.GetAnswer(m.toid.nick, m.fromid.nick, m.summary);
                    Desk.Inst.AddConversation(m.toid.nick, m.fromid.nick, m.summary, answer, isAutoReply);

                    if (isAutoReply)
                    {
                        await rpa.SendTextAsync(m.fromid.nick, answer);
                    }
                }
                await System.Threading.Tasks.Task.Delay(2000);
            });

        }


        private void Cdp_EvMessageNotity(object sender, MessageNotifyEventArgs e)
        {
            if (EvMessageNotity != null)
            {
                EvMessageNotity(this, e);
            }
        }

        private void Cdp_EvBuyerSwitched(object sender, BuyerSwitchedEventArgs e)
        {
            Seller = e.Seller;
            Buyer = e.Buyer;
            Desk.Inst.ChangeBuyer(e.Buyer.Nick);
            if (EvBuyerSwitched != null)
            {
                EvBuyerSwitched(this, e);
            }
        }

        public static QN GetByNick(LocalUser seller)
        {
            var qn = QNSet.FirstOrDefault(q => q._seller.Nick == seller.Nick || q._seller.Display == seller.Display);
            if (qn == null)
            {
                qn = new QN(seller);
                QNSet.Add(qn);
            }
            return qn;
        }

        public void SendTimiMsg(string userId, string smartTip)
        {
            cdp.SendTimiMsg(userId, smartTip);
        }


        public void TransferContact(string contactID, string targetID, string reason = "")
        {
            cdp.TransferContact(contactID, targetID, reason);
        }

        public void LightOff(string ccode)
        {
            cdp.LightOff(ccode);
        }

        public void MarkRead(string ccode, string clientId, string messageId)
        {
            cdp.MarkRead(ccode, clientId, messageId);
        }

        public async Task<LocalUserResponse> GetCurrentUser()
        {
            return await cdp.GetCurrentUser();
        }

        public void InsertText2Inputbox(string uid, string text)
        {
            cdp.InsertText2Inputbox(uid, text);
        }

        public async Task<bool> IsInputboxEmpty()
        {
            return await cdp.IsInputboxEmpty();
        }

        public void BrowserUrl(string url)
        {
            cdp.BrowserUrl(url);
        }

        public void SendRemindPayCard(string encryptedBuyerId, string orderId)
        {
            cdp.SendRemindPayCard(encryptedBuyerId, orderId);
        }

        public void RecallMessage(string ccode, string clientId, string messageId)
        {
            cdp.RecallMessage(ccode, clientId, messageId);
        }

        public void OpenChat(string nick)
        {
            cdp.OpenChat(nick);
        }

        public void GetRemoteHisMsg(string ccode)
        {
            cdp.GetRemoteHisMsg(ccode);
        }


        public void SendCoupon(string buyerNick, string activityId)
        {
            cdp.SendCoupon(buyerNick, activityId);
        }

        public void CloseChat(string contactID)
        {
            cdp.CloseChat(contactID);
        }


        public async Task<AccountStatusResponse> GetAccountStatus()
        {
            return await cdp.GetAccountStatus();
        }

        public async Task<ItemRecordResponse> GetItemRecords(string encryptId)
        {
            return await cdp.GetItemRecords(encryptId);
        }


        public async Task<SearchUserResponse> SearchBuyerUser(string searchQuery)
        {
            return await cdp.SearchBuyerUser(searchQuery);
        }

        public async Task<BuyerInfoResponse> GetBuyerInfo(string encryptId)
        {
            return await cdp.GetBuyerInfo(encryptId);
        }

        public async Task<ZnkfTradeQueryResponse> GetBuyerTrades(string securityBuyerUid, string bizOrderId)
        {
            return await cdp.GetBuyerTrades(securityBuyerUid, bizOrderId);
        }

        public async Task<ConversationResponse> GetCurrentConversationID()
        {
            var res = await cdp.GetCurrentConversationID();
            if (CurQN == null && res.Result != null && (Buyer == null || Buyer.Nick != res.Result.Nick))
            {
                Buyer = res.Result;
                CurQN = this;
                Desk.Inst.ChangeBuyer(Buyer.Nick);
            }
            return res;
        }

    }
}
