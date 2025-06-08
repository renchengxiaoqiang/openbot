using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Bot.Automation.ChatDeskNs;
using Bot.Common;
using Bot.Common.Trivial;
using BotLib;
using BotLib.Collection;
using BotLib.Extensions;
using BotLib.Misc;
using DbEntity;
using DbEntity.Conv;
using DbEntity.Response;
using DbEntity.Trade;
using MasterDevs.ChromeDevTools.Protocol.Chrome.CSS;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Debugger;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Network;
using Newtonsoft.Json;
using SuperSocket.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static Bot.Params;

namespace Bot.ChatRecord
{
    public class ChatDialog
    {
        private ChatDesk _desk;
        private ConcurrentDictionary<string, List<ZnkfItem>> _buyerInquiryItems;
        private ConcurrentBag<ChatMessage> _waitInviteOrderBuyers;
        private ConcurrentBag<ZnkfTrade> _waitRemindPayTrades;
        //最近发送催拍的买家和时间
        private ConcurrentDictionary<string, DateTime> _inviteOrderBuyers;
        //最近发送催付的买家的订单号和时间
        private ConcurrentDictionary<string, DateTime> _remindPayTrades;
        private bool _remindPayRuning = false;
        private bool _inviteOrderRuning = false;
        private NoReEnterTimer _timerInviteOrder;
        private NoReEnterTimer _timerRemindPay;
        private SynableImageHelper _imageHelper = new SynableImageHelper(TransferFileTypeEnum.RuleAnswerImage);
        public ChatDialog(ChatDesk desk)
        {
            _desk = desk;

            _waitInviteOrderBuyers = new ConcurrentBag<ChatMessage>();
            _waitRemindPayTrades = new ConcurrentBag<ZnkfTrade>();
            _buyerInquiryItems = new ConcurrentDictionary<string, List<ZnkfItem>>();
            _inviteOrderBuyers = new ConcurrentDictionary<string, DateTime>();
            _remindPayTrades = new ConcurrentDictionary<string, DateTime>();

            _timerInviteOrder = new NoReEnterTimer(LoopInviteOrder, 300, 10000);
            _timerRemindPay = new NoReEnterTimer(LoopRemindPay, 300, 10000);

            _desk.EvRecieveNewMessage += Desk_EvRecieveNewMessage;
            _desk.EvBenchMessageNotity += Desk_EvBenchMessageNotity;
        }

        private async void Desk_EvBenchMessageNotity(object sender, ChromeNs.MessageNotifyEventArgs e)
        {
            var notifyRes = JsonConvert.DeserializeObject<MessageNotifyResponse>(e.NotifyContent);
            if (notifyRes.topic == "trade")
            {
                var trds = await _desk.GetBuyerTrades(notifyRes.FromServer.Notify.Alert.buyer_UID,notifyRes.bizId);
                AddBuyerTrades(trds, notifyRes.FromServer.Notify.Alert.buyer_UID);

                //自动发货
                var isAutoSendGoods = Params.AutoSendGoods.GetOpenAutoSendGoods(_desk.SellerMainNick);
                if (isAutoSendGoods)
                {
                    var trd = trds.FirstOrDefault(k => k.bizOrderId == notifyRes.bizId);
                    if (trd.payTime.HasValue && trd.cardTypeText == "待发货")
                    {
                        await _desk.TradeDummySend(trd.bizOrderId, string.Join(",", trd.itemList.Select(k => k.subOrderId)));
                        var text = Params.AutoSendGoods.GetSendTextContent(_desk.SellerMainNick);
                        var fn = Params.AutoSendGoods.GetSendImagePath(_desk.SellerMainNick);
                        var imagePath = _imageHelper.GetImagePath(fn);

                        Log.Info(string.Format("订单自动发货,单号:{0}", trd.bizOrderId));

                        AddAutoTask(string.Empty, notifyRes.FromServer.Notify.Alert.buyer_nick
                            , notifyRes.FromServer.Notify.Alert.buyer_UID
                            , _desk.SellerNick,_desk.SellerId
                            ,trd.bizOrderId, text, "自动发货任务");


                        var acct = _desk.GetBuyerForTid(trd.bizOrderId).Result;
                        var buyerNick = acct.Nick;
                        var conv = _desk.GetConvCodeViaNick(buyerNick).Result;
                        if (conv != null)
                        {
                            if (!string.IsNullOrEmpty(text))
                            {
                                _desk.SendCcodeNativeMsg(conv.result.ccode, text);
                                Thread.Sleep(100);
                            }
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                _desk.SendCcodeNativeImage(conv.result.ccode, imagePath);
                                Thread.Sleep(50);
                            }
                        }
                    }
                }
            }
            if (notifyRes.topic == "refund")
            {
                //var trds = await GetBuyerTrades(res.FromServer.MoreContent.buyer_id);
                //Dialog.AddBuyerTrades(trds);
            }
        }

        private void Desk_EvRecieveNewMessage(object sender, ChromeNs.RecieveNewMessageEventArgs e)
        {
            var chatRes = JsonConvert.DeserializeObject<ChatResponse>(e.Message);
            var messages = chatRes.result;
            AddBuyerMessages(messages);
        }

        private void LoopInviteOrder()
        {
            if (!_inviteOrderRuning)
            {
                _inviteOrderRuning = true;
                try
                {
                    var afterMinutes = Params.InviteOrder.GetSendAfterForMinute(_desk.SellerMainNick);
                    if (afterMinutes < 0) return;
                    foreach (var msg in _waitInviteOrderBuyers)
                    {
                        var sendTime = long.Parse(msg.sendTime).xTimeStampToDate();
                        var ccode = msg.cid.ccode;
                        var buyerId = msg.fromid.targetId; //ParseCCode(ccode, _desk.SellerId);
                        
                        var latestInviteOrderTime = _inviteOrderBuyers.GetOrAdd(buyerId, key => DateTime.MinValue);
                        if (sendTime.xIsTimeElapseMoreThanMinute(afterMinutes) && latestInviteOrderTime.xIsTimeElapseMoreThanHours(24))
                        {
                            var buyerInquiryItems = GetBuyerInquiryItem(buyerId);
                            var inquiryItem = InviteOrder.GetFilterInquiryItem(_desk.SellerMainNick);
                            if (!string.IsNullOrEmpty(inquiryItem))
                            {
                                var itemIds = inquiryItem.xSplitByComma();
                                buyerInquiryItems = buyerInquiryItems.Where(bi => itemIds.Any(id => id == bi.itemId.ToString())).ToList();
                            }

                            if (buyerInquiryItems.Count > 0 && IsMatchBuyerInfo(buyerId))
                            {
                                var text = Params.InviteOrder.GetSendTextContent(_desk.SellerMainNick);
                                var fn = Params.InviteOrder.GetSendImagePath(_desk.SellerMainNick);
                                var imagePath = _imageHelper.GetImagePath(fn);
                                var msgStyle = Params.InviteOrder.GetSendMessageStyle(_desk.SellerMainNick);

                                _inviteOrderBuyers.AddOrUpdate(buyerId, key => DateTime.Now, (key, value) => DateTime.Now);
                                Log.Info(string.Format("催拍任务执行,买家id:{0}", buyerId));
                                AddAutoTask(ccode, msg.fromid.nick
                                , msg.fromid.targetId
                                , _desk.SellerNick
                                , _desk.SellerId
                                , string.Join(",", buyerInquiryItems.Select(k => k.itemId)), text, "催拍任务");
                                if (msgStyle.Contains("商品推荐卡片"))
                                {
                                    _desk.SendItemRecommendCard(buyerId, string.Join(",", buyerInquiryItems.Select(k => k.itemId)));
                                    Thread.Sleep(400);
                                }
                                if (msgStyle.Contains("话术"))
                                {
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        _desk.SendCcodeNativeMsg(ccode, text);
                                        Thread.Sleep(100);
                                    }
                                    if (!string.IsNullOrEmpty(imagePath))
                                    {
                                        _desk.SendCcodeNativeImage(ccode, imagePath);
                                        Thread.Sleep(50);
                                    }
                                }
                            }
                        }

                    }
                }
                catch { }
                finally
                {
                    _inviteOrderRuning = false;
                }

            }

        }

        public bool IsMatchBuyerInfo(string buyerId)
        {
            var rt = true;
            var buyerLevel = InviteOrder.GetFilterBuyerLevel(_desk.SellerMainNick);
            var buyerType = InviteOrder.GetFilterBuyerType(_desk.SellerMainNick);
            var buyAmountGt = InviteOrder.GetFilterBuyAmountGt(_desk.SellerMainNick);
            var sendGoodRate = InviteOrder.GetFilterSendGoodsRate(_desk.SellerMainNick);

            if(!string.IsNullOrEmpty(buyerLevel) 
                || !string.IsNullOrEmpty(buyerType)
                || buyAmountGt > 0 || sendGoodRate > 0)
            {
                //vipLevel =10 超级vip
                var buyerRes = _desk.GetBuyerInfo(buyerId).Result;
                var buyer = buyerRes.Data;

                if (!string.IsNullOrEmpty(buyerLevel))
                {
                    if (buyerLevel == "超级会员" && buyer.vipLevel < 10) { rt = false; }
                    if (buyerLevel == "普通会员" && buyer.vipLevel >= 10) { rt = false; }
                }
                if (!string.IsNullOrEmpty(buyerType))
                {
                    if (buyerType == "新客户" && !buyer.IsNewCustomer) { rt = false; }
                    if (buyerType == "老客户" && buyer.IsNewCustomer) { rt = false; }
                }
                if (buyAmountGt > 0)
                {
                    rt = (buyer.TradeAmount ?? 0) <= buyAmountGt;
                }
                if (sendGoodRate > 0)
                {
                    var sendGoodsRateNum = decimal.Parse(buyer.SendGoodRate.Replace("%",""));
                    rt = sendGoodsRateNum <= sendGoodRate;
                }
            }

            return rt;
        }

        private List<ZnkfItem> GetBuyerInquiryItem(string buyerId)
        {
            var buyerInquiryItems = _buyerInquiryItems.GetOrAdd(buyerId, (key) => new List<ZnkfItem>());
            if (buyerInquiryItems.Count < 1)
            {
                var itemRecordRes = _desk.GetItemRecords(buyerId).Result;
                var hasInquiryItem = itemRecordRes != null && itemRecordRes.data != null
                && itemRecordRes.data.underInquiryItemList != null
                && itemRecordRes.data.underInquiryItemList.Count > 0;
                if (hasInquiryItem)
                {
                    _buyerInquiryItems.AddOrUpdate(buyerId, key => itemRecordRes.data.underInquiryItemList
                    , (key, value) => itemRecordRes.data.underInquiryItemList);
                }
            }
            return buyerInquiryItems;
        }

        public string ParseCCode(string ccode, string sellerId)
        {
            var buyerId = string.Empty;
            var match = Regex.Match(ccode, "^(\\d+)\\.\\d+-(\\d+)\\.\\d+#(\\d+)@(.*)$");
            if (match.Success)
            {
                var id1 = match.Groups[1];
                var id2 = match.Groups[2];
                buyerId = id1.ToString() == sellerId ? id2.ToString() : id1.ToString();
            }
            return buyerId;
        }

        private void LoopRemindPay()
        {
            if (!_remindPayRuning)
            {
                _remindPayRuning = true;
                try
                {
                    var afterMinutes = Params.RemindPay.GetSendAfterForMinute(_desk.SellerMainNick);
                    if (afterMinutes < 0) return;

                    foreach (var trd in _waitRemindPayTrades)
                    {
                        var latestRemindPayTime = _remindPayTrades.GetOrAdd(trd.bizOrderId, key => DateTime.MinValue);
                        if (!trd.payTime.HasValue && trd.createTime.xIsTimeElapseMoreThanMinute(afterMinutes)
                        && latestRemindPayTime.xIsTimeElapseMoreThanHours(24)
                        && IsMatchTrade(trd))
                        {
                            var acct = _desk.GetBuyerForTid(trd.bizOrderId).Result;
                            var buyerNick = acct.Nick;
                            var searchUserResponse = _desk.GetBuyerId(buyerNick).Result;
                            var conv = _desk.GetConvCodeViaNick(buyerNick).Result;

                            _remindPayTrades.AddOrUpdate(trd.bizOrderId, key => DateTime.Now, (key, value) => DateTime.Now);
                            Log.Info(string.Format("催付任务执行,买家昵称:{0}", buyerNick));
                            

                            var text = Params.RemindPay.GetSendTextContent(_desk.SellerMainNick);
                            var fn = Params.RemindPay.GetSendImagePath(_desk.SellerMainNick);
                            var imagePath = _imageHelper.GetImagePath(fn);
                            var msgStyle = Params.RemindPay.GetSendMessageStyle(_desk.SellerMainNick);
                            
                            AddAutoTask(conv.result.ccode, buyerNick
                            , searchUserResponse.Data.Data[0].AccountId
                            , _desk.SellerNick
                            , _desk.SellerId
                            , trd.bizOrderId, text, "催付任务");

                            if (msgStyle.Contains("催付卡片"))
                            {
                                string tid = trd.bizOrderId;
                                string payment = trd.orderPrice;
                                string tradeNum = trd.buyAmount.ToString();
                                string orderTitle = trd.itemList[0].auctionTitle;
                                string orderPicUrl = trd.itemList[0].picUrl;
                                _desk.SendRemindPayCard(searchUserResponse.Data.Data[0].AccountId, tid);
                                Thread.Sleep(400);
                            }

                            if (conv != null && msgStyle.Contains("话术"))
                            {
                                if (!string.IsNullOrEmpty(text))
                                {
                                    _desk.SendCcodeNativeMsg(conv.result.ccode, text);
                                    Thread.Sleep(100);
                                }
                                if (!string.IsNullOrEmpty(imagePath))
                                {
                                    _desk.SendCcodeNativeImage(conv.result.ccode, imagePath);
                                    Thread.Sleep(50);
                                }
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    _remindPayRuning = false;
                }
            }
        }

        public bool IsMatchTrade(ZnkfTrade trade)
        {
            var rt = true;

            var sellerFlag = RemindPay.GetFilterSellerFlag(_desk.SellerMainNick);
            var sellerMemo = RemindPay.GetFilterSellerMemo(_desk.SellerMainNick);
            var tradeItem = RemindPay.GetFilterTradeItem(_desk.SellerMainNick);
            var tradePayAmountGt = RemindPay.GetFilterTradePayAmountGt(_desk.SellerMainNick);

            if (!string.IsNullOrEmpty(sellerMemo)
                || !string.IsNullOrEmpty(tradeItem)
                || sellerFlag > -1 || tradePayAmountGt > 0)
            {

                if (!string.IsNullOrEmpty(sellerMemo))
                {
                    rt = !trade.sellerMemo.Contains(sellerMemo);
                }
                if (!string.IsNullOrEmpty(tradeItem))
                {
                    var itemIds = tradeItem.xSplitByComma();
                    rt = !trade.itemList.Any(bi => itemIds.Any(id => id == bi.auctionId.ToString()));
                }
                if (sellerFlag > 0)
                {
                    rt = sellerFlag != trade.sellerFlag;
                }
                if (tradePayAmountGt > 0)
                {
                    rt = tradePayAmountGt <= trade.buyAmount;
                }
            }

            return rt;
        }

        private void AddBuyerMessages(List<ChatMessage> messages)
        {
            if (messages == null || messages.Count < 1) return;
            messages.ForEach(msg =>
            {
                //只记录买家主动发送的消息
                if (msg.fromid.targetId != _desk.SellerId)
                {
                    //相同买家只最近一条聊天记录
                    _waitInviteOrderBuyers = _waitInviteOrderBuyers.xRemoveAll(k => k.cid.ccode == msg.cid.ccode);
                    _waitInviteOrderBuyers.Add(msg);
                }
            });
        }

        private void AddBuyerTrades(List<ZnkfTrade> trades, string buyerId)
        {
            if (trades == null || trades.Count < 1) return;

            //记录主动的下单的客户，不要重复催拍
            _inviteOrderBuyers.AddOrUpdate(buyerId, key => DateTime.Now, (key, value) => DateTime.Now);

            AddAutoTask(string.Empty, string.Empty
            , buyerId
            , _desk.SellerNick
            , _desk.SellerId
            , string.Join(",",trades.Select(k=>k.bizOrderId)), "订单通知，取消催付任务", "催付任务");

            var nopayTrades = trades.Where(k => k.cardTypeText == "未付款").OrderBy(k => k.createTime).ToList();
            var otherTrades = trades.Where(k => k.cardTypeText != "未付款").OrderBy(k => k.createTime).ToList();
            nopayTrades.ForEach(trd =>
            {
                _waitRemindPayTrades.Add(trd);
            });
            //移除不是 未付款 状态的订单，不用催付
            _waitRemindPayTrades = _waitRemindPayTrades.xRemoveAll(trd => otherTrades.Any(t => t.bizOrderId == trd.bizOrderId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ccode"></param>
        /// <param name="buyer"></param>
        /// <param name="buyerId"></param>
        /// <param name="seller"></param>
        /// <param name="sellerId"></param>
        /// <param name="tid"></param>
        /// <param name="summary"></param>
        /// <param name="taskType"></param>
        private void AddAutoTask(string ccode, string buyer,string buyerId
            ,string seller,string sellerId,string tid,string summary
            ,string taskType)
        {
            var et = EntityHelper.Create<AutoTaskEntity>(TbNickHelper.ConvertNickToPubDbAccount(_desk.SellerNick));
            et.Ccode = ccode;
            et.RecvUser = buyer;
            et.RecvUserId = buyerId;
            et.SendTime = DateTime.Now;
            et.SendUser = seller;
            et.SendUserId = sellerId;
            et.Summary = summary;
            et.Tid = tid;
            et.TaskType = taskType;
            DbHelper.SaveToDb(et);
        }
    }
}

