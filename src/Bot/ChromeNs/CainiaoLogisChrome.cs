using BotLib;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BotLib.Extensions;
using Bot.Automation.ChatDeskNs;
using Bot.Common;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Top.Api;
using System.Collections.Concurrent;
using Top.Api.Util;
using System.Threading.Tasks;
using System.Web;
using Top.Api.Response;
using Top.Api.Domain;
using DbEntity;
using Newtonsoft.Json;
using BotLib.Misc;

namespace Bot.ChromeNs
{
    public class ChatRecordChrome : ChromeConnector
    {
        private HashSet<string> _chatRecordChromeTitle;
        private DateTime _preListeningTime;
        public event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        public event EventHandler<RecieveNewMessageEventArgs> EvRecieveNewMessage;
        public event EventHandler<ShopRobotReceriveNewMessageEventArgs> EvShopRobotReceriveNewMessage;
        public string PreBuyer
        {
            get;
            private set;
        }
        public string CurBuyer
        {
            get;
            private set;
        }


        private ConcurrentDictionary<long, ManualResetEventSlim> _requestWaitHandles = new ConcurrentDictionary<long, ManualResetEventSlim>();
        private ConcurrentDictionary<long, TopResponse> _responses = new ConcurrentDictionary<long, TopResponse>();
        private ConcurrentDictionary<long, string> _imsdkResponses = new ConcurrentDictionary<long, string>();
        long _incrementCount = 0;

        public ChatRecordChrome(ChatDesk desk)
            : base(desk.Hwnd.Handle, "nick_" + desk.Seller)
        {
            _chatRecordChromeTitle = new HashSet<string>
			{
				"当前聊天窗口",
				"IMKIT.CLIENT.QIANNIU",
				"聊天窗口",
				"imkit.qianniu",
                "千牛聊天消息",
                "千牛消息聊天"
			};
            _preListeningTime = DateTime.Now;
            Timer.AddAction(FetchRecordLoop, 300, 300);
            //Timer = new NoReEnterTimer(FetchRecordLoop, 300, 300);
        }
        public bool GetHtml(out string html, int timeoutMs = 500)
        {
            html = "";
            bool result = false;
            if (WaitForChromeOk(timeoutMs))
            {
                result = true;
            }
            return result;
        }
        protected override void ClearStateValues()
        {
            base.ClearStateValues();
            CurBuyer = "";
            PreBuyer = "";
        }

        protected override ChromeOperator CreateChromeOperator(string chromeSessionInfoUrl)
        {
            var chromeOp = new ChromeOperator(chromeSessionInfoUrl, _chatRecordChromeTitle, true);
            chromeOp.ClearChromeConsole();
            //chromeOp.EvalForReplaceQnMinJs();
            chromeOp.ListenChromeConsoleMessageAddedMessage(DealChromeConsoleMessage);
            chromeOp.EvalForMessageListen();
            chromeOp.EvalForStartJs();
            return chromeOp;
        }

        private void DealChromeConsoleMessage(ConsoleMessage consoleMessage)
        {
            try
            {
                var text = consoleMessage.Text.Trim();
                if (text.StartsWith("ChromeWindowConsoleLog,"))
                {
                    TopResponse response = null;
                    var t = GetType();
                    var funcGetTopResponse = GetType().GetMethod("GetTopResponse", BindingFlags.NonPublic | BindingFlags.Instance);
                    text = text.Substring("ChromeWindowConsoleLog,".Length);
                    var jObject = JObject.Parse(text);
                    var commandId = long.Parse(jObject["task_command_id"].ToString());
                    var apiName = jObject["api_name"].ToString();
                    var topResponse = jObject["top_response"].ToString();
                    var resT = GetTopResponseGenericMethodType(apiName);
                    var GeneriGetTopResponse = funcGetTopResponse.MakeGenericMethod(resT);
                    var res = GeneriGetTopResponse.Invoke(this, new object[] { topResponse });
                    if (res != null)
                    {
                        response = res as TopResponse;
                        HandleResponse(response, commandId);
                    }
                }
                else if (text.StartsWith("ChromeImSdkConsoleLog,"))
                {
                    text = text.Substring("ChromeImSdkConsoleLog,".Length); 
                    var jObject = JObject.Parse(text);
                    var commandId = long.Parse(jObject["task_command_id"].ToString());
                    var apiName = jObject["api_name"].ToString();
                    var sdkResponse = jObject["sdk_response"].ToString();
                    HandleResponse(sdkResponse, commandId);
                }
                else if (text.StartsWith("onConversationChange,"))
                {
                    var buyer = text.Substring("onConversationChange,".Length);
                    BuyerSwitched(buyer, null);
                }
                else if (text.StartsWith("onSendNewMsg,"))
                {
                    var buyer = text.Substring("onSendNewMsg,".Length);
                    if (string.IsNullOrEmpty(CurBuyer) || CurBuyer == buyer)
                    {
                    }
                }
                else if (text.StartsWith("onReceiveNewMsg,"))
                {
                    var msgResponse = text.Substring("onReceiveNewMsg,".Length);
                    RecieveNewMessage(msgResponse);
                }
                else if (text.StartsWith("onShopRobotReceriveNewMsgs,"))
                {
                    var buyer = text.Substring("onShopRobotReceriveNewMsgs,".Length);
                    ShopRobotReceriveNewMessage(buyer);
                }
                else if (text.StartsWith("onConversationAdd,"))
                {
                    Util.WriteTrace("ChromeMessageConsumer" + text);
                }
                else if (text.StartsWith("onConversationClose,"))
                {
                    var buyer = text.Substring("onConversationClose,".Length);
                }
                else if (text.StartsWith("onNetDisConnect,"))
                {
                    Log.WriteLine("ChromeMessageConsumer:" + text, new object[0]);
                }
                else if (text.StartsWith("onNetReConnectOK,"))
                {
                    Util.WriteTrace("ChromeMessageConsumer" + text);
                    Log.WriteLine("ChromeMessageConsumer:" + text, new object[0]);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            finally {
                
            }
        }
        public Type GetTopResponseGenericMethodType(string apiName)
        {
            //Type req = null;
            //foreach (var k in reqNs)
            //{
            //    var inst = k.Assembly.CreateInstance(k.FullName);
            //    var m = k.GetMethod("GetApiName");
            //    if (m == null) continue;
            //    var r = m.Invoke(inst, null);
            //    if (r != null && r.ToString() == apiName)
            //    {
            //        req = k;
            //        break;
            //    }
            //}
            //var responseGenericMethodType = req.BaseType.GenericTypeArguments[0].FullName;
            var ts = Assembly.Load("TopSdk");
            var reqNs = ts.DefinedTypes.Where(k => k.Namespace == "Top.Api.Request");
            var req = reqNs.FirstOrDefault(k => k.GetMethod("GetApiName") != null && k.GetMethod("GetApiName").Invoke(k.Assembly.CreateInstance(k.FullName), null).ToString() == apiName);
            return req.BaseType.GenericTypeArguments[0];
        }

        private TopResponse GetTopResponse<T>(string message) where T : TopResponse
        {
            return TopUtils.ParseResponse<T>(message);
        }

        private void HandleResponse(string response, long commandId)
        {
            if (null == response) return;
            ManualResetEventSlim requestMre;
            if (_requestWaitHandles.TryGetValue(commandId, out requestMre))
            {
                _imsdkResponses.AddOrUpdate(commandId, id => response, (key, value) => response);
                requestMre.Set();
            }
            else
            {
                if (1 == _requestWaitHandles.Count)
                {
                    var requestId = _requestWaitHandles.Keys.First();
                    _requestWaitHandles.TryGetValue(requestId, out requestMre);
                    _imsdkResponses.AddOrUpdate(requestId, id => response, (key, value) => response);
                    requestMre.Set();
                }
            }
        }
        private void HandleResponse(TopResponse response, long commandId)
        {
            if (null == response) return;
            ManualResetEventSlim requestMre;
            if (_requestWaitHandles.TryGetValue(commandId, out requestMre))
            {
                _responses.AddOrUpdate(commandId, id => response, (key, value) => response);
                requestMre.Set();
            }
            else
            {
                if (1 == _requestWaitHandles.Count)
                {
                    var requestId = _requestWaitHandles.Keys.First();
                    _requestWaitHandles.TryGetValue(requestId, out requestMre);
                    _responses.AddOrUpdate(requestId, id => response, (key, value) => response);
                    requestMre.Set();
                }
            }
        }

        private void RecieveNewMessage(string msg)
        {
            if (EvRecieveNewMessage != null)
            {
                EvRecieveNewMessage(this, new RecieveNewMessageEventArgs
                {
                    Buyer = string.Empty,
                    Connector = this,
                    Message = msg
                });
            }
        }

        private void ShopRobotReceriveNewMessage(string buyer)
        {
            if (EvShopRobotReceriveNewMessage != null)
            {
                EvShopRobotReceriveNewMessage(this, new ShopRobotReceriveNewMessageEventArgs
                {
                    Buyer = buyer,
                });
            }
        }

        private void FetchRecordLoop()
        {
            try
            {
                if (IsChromeOk)
                {
                    if ((DateTime.Now - _preListeningTime).TotalSeconds > 5.0)
                    {
                        _preListeningTime = DateTime.Now;
                        if (ChromOp != null)
                        {
                            ChromOp.EvalForMessageListen();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void BuyerSwitched(string preBuyer, string curBuyer = null)
        {
            if (preBuyer != curBuyer)
            {
                PreBuyer = (curBuyer ?? CurBuyer);
                CurBuyer = preBuyer;
                if (EvBuyerSwitched != null)
                {
                    EvBuyerSwitched(this, new BuyerSwitchedEventArgs
                    {
                        CurBuyer = preBuyer,
                        PreBuyer = PreBuyer,
                        Connector = this
                    });
                }
            }
        }

        public void ExcuteConsoleCmd(string cmd)
        {
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public async Task<string> ExcuteConsoleCmd(string cmd, string apiName)
        {
            cmd = cmd + ".then(res=>console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: res})));";
            var taskId = Interlocked.Increment(ref _incrementCount);
            cmd = cmd.Replace("{api_name}", apiName)
                 .Replace("{task_command_id}", taskId.ToString());
            return await RequestQnApi(cmd, taskId);
        }

        public async Task<TopResponse> ExcuteTopRequest(string apiName, Dictionary<string, string> pms)
        {
            return await RequestTopApi(apiName, pms);
        }

        public void SendMsg(string uid, string msg)
        {
            var cmd = @"
                imsdk.invoke('intelligentservice.SendSmartTipMsg', {
                    userId: '{uid}',
                    smartTip: decodeURIComponent('{msg}')
                }).then((data)=>{
                    
                });";
            cmd = cmd.Replace("{uid}", uid).Replace("{msg}", HttpUtility.HtmlEncode(msg));
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public void TransferContact(string contactID, string targetID,string reason="")
        {
            var cmd = @"
                imsdk.invoke('application.openChat',{nick:'{contactID}'})
                .then((data)=>{
                    imsdk.invoke('application.transferContact', {
                        contactID: '{contactID}',
                        targetID: '{targetID}',
                        reason: '{reason}'
                    });
                });";
            cmd = cmd.Replace("{contactID}", "cntaobao" + contactID).Replace("{targetID}", "cntaobao"+targetID).Replace("{reason}", reason);
            if (ChromOp != null)
            {
                OpenChat(contactID);
                ChromOp.Eval(cmd);
            }
        }

        public void InsertText2Inputbox(string uid, string text)
        {
            var cmd = @"
                QN.app.invoke({
                    api: 'insertText2Inputbox',
                    query: {
                      uid: '{uid}',
                      text : decodeURIComponent('{text}')  
                    },
                  });";
            cmd = cmd.Replace("{uid}", "cntaobao"+uid).Replace("{text}", HttpUtility.HtmlEncode(text));
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public void SendCheckOrderCard(string tid, string buyer)
        {
            var cmd = @"
                imsdk.invoke('application.invokeMTopChannelService', {
                    method: 'mtop.taobao.bcpush.order.check.card.send',
                    param:{bizOrderId:'{tid}',buyerNick:'{buyer}'},
                    httpMethod: 'post',
                    version: '1.0'
                });";
            cmd = cmd.Replace("{tid}", tid).Replace("{buyer}", buyer);
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public async Task<TradeDecryptResponse> GetTradeDecrypt(string tid)
        {
            var pms = new Dictionary<string, string>();
            pms["bizType"] = "qianniu";
            pms["tid"] = tid;
            pms["queryByTid"] = "true";
            var mtopRes = await ExcuteMTopCmd("mtop.taobao.tid.decrypt", pms, "1.0");
            return JsonConvert.DeserializeObject<TradeDecryptResponse>(mtopRes);
        }


        public async Task<string> ExcuteMTopCmd(string apiName, Dictionary<string, string> pms, string version = "1.0")
        {
            var paramStr = string.Empty;
            var pmList = new List<string>();
            if (pms != null && pms.Count > 0)
            {
                var idx = 0;
                foreach (var key in pms.Keys)
                {
                    var val = pms[key];
                    pmList.Add(string.Format("'{0}': '{1}'", key, val));
                    idx++;
                }
            }
            var cmd = @"QN.app.invoke({
                api: 'invokeMTopChannelService',
                query: {
                  method: '{api_name}',
                  param: {{param}},
                  httpMethod: 'post',
                  version: '{version}',
                },
              }).then(res=>console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: res})));";
            var taskId = Interlocked.Increment(ref _incrementCount);

            paramStr = pmList.Aggregate((s1, s2) => s1 + ", " + s2);
            cmd = cmd.Replace("{api_name}", apiName)
                  .Replace("{version}", version)
                  .Replace("{param}", paramStr)
                 .Replace("{task_command_id}", taskId.ToString());
            return await RequestQnApi(cmd, taskId);
        }

        public void SendRemindPayCard(string buyer,string tid,string payment,string tradeNum, string orderTitle,string orderPicUrl)
        {
            var cmd = @"
                imsdk.invoke('application.invokeMTopChannelService', {
                    method: 'mtop.taobao.dgw.card.send',
                    param:{
                            data: JSON.stringify({
                            cardCode: 'znkf_remind_pay_new',
                            cardParams:{
                                        title: '亲，您有一笔订单未付款',
                                        logo: '{orderPicUrl}',
                                        subTitle: '{orderTitle}',
                                        description: '合计￥{payment}',
                                        btnRight: '付款',
                                        //remark: '',phone: '13162548564',name: '张先生',fulladdress: '123',
                                        btnRightAction: 'https://tm.m.taobao.com/order/order_detail.htm?bizOrderId={tid}',
                                        extActionUrl: 'https://tm.m.taobao.com/order/order_detail.htm?bizOrderId={tid}'
                                    },
                                    bizUUid: '{uuid}',
                                    appkey: 23574652
                            }),
                            receiver: '{buyer}',
                            domain: 'cntaobao'
                    },
                    httpMethod: 'post',
                    version: '1.0'
                }); ";
            cmd = cmd.Replace("{tid}", tid).Replace("{buyer}","cntaobao"+ buyer)
                .Replace("{orderPicUrl}", orderPicUrl)
                .Replace("{orderTitle}", orderTitle)
                .Replace("{tradeNum}", tradeNum)
                .Replace("{payment}", payment)
                .Replace("{uuid}", Guid.NewGuid().ToString());
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public void SendMemberCard(string seller, string buyer)
        {
            var cmd = @"
                QN.app.invoke({
                    api: 'invokeMTopChannelService',
                    query: {
                      method: 'mtop.taobao.seattle.qianniu.card.send.post',
                      param: JSON.stringify({sellerNick: '{seller}',buyerNick: '{buyer}'}),
                      httpMethod: 'post',
                      version: '1.0',
                    },
                  }); ";
            cmd = cmd.Replace("{seller}", "cntaobao"+seller).Replace("{buyer}", "cntaobao"+buyer);
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public void BrowserUrl(string url)
        {
            var cmd = @"
                QN.app.invoke( {
                    api : 'browserUrl',
                    query : {
                            url : '{url}'  
                            
                    }
                });";
            cmd = cmd.Replace("{url}",url);
            var retVal = string.Empty;
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd, out retVal);
            }
        }

        public void RecallMessage(string ccode,string clientId,string messageId)
        {
            var cmd = @"
                imsdk.invoke('im.singlemsg.DoChatMsgWithdraw', {
                    cid: {ccode:'{ccode}'},
                    mcodes: [{clientId:'{clientId}',messageId:'{messageId}'}]
                }).then((data)=>{
                    
                });";
            cmd = cmd.Replace("{ccode}", ccode).Replace("{clientId}", clientId).Replace("{messageId}", messageId);
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public void OpenChat(string uid)
        {
            var cmd = @"
                imsdk.invoke('application.openChat',{nick:'{uid}'})
                .then((data)=>{
                    console.log(data);
                });";
            cmd = cmd.Replace("{uid}", uid);
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public void SendCoupon(string uid,string activityId)
        {
            var cmd = @"
                QN.app.invoke({
                    api: 'invokeMTopChannelService',
                    query: {
                      method: 'mtop.taobao.qianniu.airisland.coupon.send.card',
                      param: {activityId:'{activityId}',buyerNick:'{uid}'},
                      httpMethod: 'post',
                      version: '1.0',
                    },
                }); ";
            cmd = cmd.Replace("{uid}", uid).Replace("{activityId}", activityId);
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public void CloseChat(string uid)
        {
            var cmd = @"
                imsdk.invoke('application.closeChat',{contactID:'{uid}'})
                .then((data)=>{
                    console.log(data);
                });";
            cmd = cmd.Replace("{uid}", uid);
            if (ChromOp != null)
            {
                ChromOp.Eval(cmd);
            }
        }

        public async Task<TopResponse> TradesSoldIncrementGet(DateTime startModified, DateTime endModified)
        {
            var pms = new Dictionary<string, string>();
            pms["fields"] = "storeId,promise_service,collect_time,delivery_time,dispatch_time,sign_time,cutoff_minutes,es_date,es_range,os_date,os_range,timing_promise,num,buyer_alipay_no,step,modified,timeout_action_time,end_time,pay_time,consign_time,rate_status,seller_nick,shipping_type,cod_status,orders.oid,orders.oid_str,orders.outer_iid,orders.outer_sku_id,orders.consign_time,tid,tid_str,status,end_time,buyer_nick,trade_from,credit_card_fee,buyer_rate,seller_rate,created,num,payment,pic_path,has_buyer_message,receiver_country,receiver_state,receiver_city,receiver_district,receiver_town,receiver_address,receiver_zip,receiver_name,receiver_mobile,receiver_phone,orders.timeout_action_time,orders.end_time,orders.title,orders.status,orders.price,orders.payment,orders.sku_properties_name,orders.num_iid,orders.refund_id,orders.pic_path,orders.refund_status,orders.num,orders.logistics_company,orders.invoice_no,orders.adjust_fee,seller_flag,type,post_fee,has_yfx,yfx_fee,buyer_message,buyer_flag,buyer_memo,seller_memo,orders.seller_rate,adjust_fee,invoice_name,invoice_type,invoice_kind,promotion_details,alipay_no,buyerTaxNO,pbly,orders,total_fee,orders.oid_str,orders.cid,service_orders.tmser_spu_code,step_trade_status,step_paid_fee,send_time,oaid";
            pms["start_modified"] = startModified.ToString("yyyy-MM-dd");
            pms["end_modified"] = endModified.ToString("yyyy-MM-dd") + " 23:59:59";
            var res = await RequestTopApi("taobao.trades.sold.increment.get", pms);
            return res;
        }

        public async Task<List<Trade>> TradesSoldGetForAllPage(DateTime startCreated, DateTime endCreated)
        {
            var trades = new List<Trade>();
            var pageNo = 1;
            var totalPages = 0L;
            var pageSize = 100;
            do
            {
                var pms = new Dictionary<string, string>();
                pms["fields"] = "storeId,promise_service,collect_time,delivery_time,dispatch_time,sign_time,cutoff_minutes,es_date,es_range,os_date,os_range,timing_promise,num,buyer_alipay_no,step,modified,timeout_action_time,end_time,pay_time,consign_time,rate_status,seller_nick,shipping_type,cod_status,orders.oid,orders.oid_str,orders.outer_iid,orders.outer_sku_id,orders.consign_time,tid,tid_str,status,end_time,buyer_nick,trade_from,credit_card_fee,buyer_rate,seller_rate,created,num,payment,pic_path,has_buyer_message,receiver_country,receiver_state,receiver_city,receiver_district,receiver_town,receiver_address,receiver_zip,receiver_name,receiver_mobile,receiver_phone,orders.timeout_action_time,orders.end_time,orders.title,orders.status,orders.price,orders.payment,orders.sku_properties_name,orders.num_iid,orders.refund_id,orders.pic_path,orders.refund_status,orders.num,orders.logistics_company,orders.invoice_no,orders.adjust_fee,seller_flag,type,post_fee,has_yfx,yfx_fee,buyer_message,buyer_flag,buyer_memo,seller_memo,orders.seller_rate,adjust_fee,invoice_name,invoice_type,invoice_kind,promotion_details,alipay_no,buyerTaxNO,pbly,orders,total_fee,orders.oid_str,orders.cid,service_orders.tmser_spu_code,step_trade_status,step_paid_fee,send_time,oaid";
                pms["start_created"] = startCreated.ToString("yyyy-MM-dd") + " 00:00:00";
                pms["end_created"] = endCreated.ToString("yyyy-MM-dd") + " 23:59:59";
                pms["page_no"] = pageNo.ToString();
                pms["page_size"] = pageSize.ToString();
                pms["type"] = "fixed,auction,guarantee_trade,step,independent_simple_trade,independent_shop_trade,auto_delivery,ec,cod,game_equipment,shopex_trade,netcn_trade,external_trade,instant_trade,b2c_cod,hotel_trade,super_market_trade,super_market_cod_trade,taohua,waimai,nopaid,eticket,tmall_i18n,o2o_offlinetrade";
                var res = (await RequestTopApi("taobao.trades.sold.get", pms)) as TradesSoldGetResponse;
                totalPages = (res.TotalResults + pageSize - 1) / pageSize;
                pageNo++;
                if (!res.IsError && res.Trades != null)
                {
                    trades.AddRange(res.Trades);
                }
            } while (pageNo <= totalPages);
            ClearChromeConsole();
            return trades;
        }

        public async Task<TopResponse> TradesSoldGet(string buyerNick)
        {
            var pms = new Dictionary<string, string>();
            pms["fields"] = "storeId,promise_service,collect_time,delivery_time,dispatch_time,sign_time,cutoff_minutes,es_date,es_range,os_date,os_range,timing_promise,num,buyer_alipay_no,step,modified,timeout_action_time,end_time,pay_time,consign_time,rate_status,seller_nick,shipping_type,cod_status,orders.oid,orders.oid_str,orders.outer_iid,orders.outer_sku_id,orders.consign_time,tid,tid_str,status,end_time,buyer_nick,trade_from,credit_card_fee,buyer_rate,seller_rate,created,num,payment,pic_path,has_buyer_message,receiver_country,receiver_state,receiver_city,receiver_district,receiver_town,receiver_address,receiver_zip,receiver_name,receiver_mobile,receiver_phone,orders.timeout_action_time,orders.end_time,orders.title,orders.status,orders.price,orders.payment,orders.sku_properties_name,orders.num_iid,orders.refund_id,orders.pic_path,orders.refund_status,orders.num,orders.logistics_company,orders.invoice_no,orders.adjust_fee,seller_flag,type,post_fee,has_yfx,yfx_fee,buyer_message,buyer_flag,buyer_memo,seller_memo,orders.seller_rate,adjust_fee,invoice_name,invoice_type,invoice_kind,promotion_details,alipay_no,buyerTaxNO,pbly,orders,total_fee,orders.oid_str,orders.cid,service_orders.tmser_spu_code,step_trade_status,step_paid_fee,send_time,oaid";
            pms["buyer_nick"] = buyerNick;
            var res = await RequestTopApi("taobao.trades.sold.get", pms);
            var soldGetRes = res as TradesSoldGetResponse;
            //if (!soldGetRes.IsError && soldGetRes.Trades != null)
            //{
            //    //获取订单详细信息
            //    for (var i = 0; i < soldGetRes.Trades.Count; i++)
            //    {
            //        var decrypt = GetTradeDecrypt(soldGetRes.Trades[i].Tid.ToString()).Result;
            //        soldGetRes.Trades[i].ReceiverAddress = decrypt.AddressDetail;
            //        soldGetRes.Trades[i].ReceiverMobile = decrypt.Mobile;
            //        soldGetRes.Trades[i].ReceiverPhone = decrypt.Phone;
            //        soldGetRes.Trades[i].ReceiverName = decrypt.Name;
            //    }
            //}
            ClearChromeConsole();
            return soldGetRes;
        }

        public async Task<TopResponse> ItemsOnsaleGet()
        {
            var pageNo = 1;
            var totalPages = 0L;
            var pageSize = 100;
            var saleItemRes = new ItemsOnsaleGetResponse();
            saleItemRes.Items = new List<Top.Api.Domain.Item>();
            do
            {
                var pms = new Dictionary<string, string>();
                pms["page_no"] = pageNo.ToString();
                pms["page_size"] = pageSize.ToString();
                pms["fields"] = "approve_status,num_iid,title,nick,type,cid,pic_url,num,props,valid_thru,list_time,price,has_discount,has_invoice,has_warranty,has_showcase,modified,delist_time,postage_id,seller_cids,outer_id,sold_quantity";
                var res = await RequestTopApi("taobao.items.onsale.get", pms) as ItemsOnsaleGetResponse;
                totalPages = (res.TotalResults + pageSize - 1) / pageSize;
                pageNo++;
                if (res.Items != null)
                {
                    saleItemRes.Items.AddRange(res.Items);
                }
            } while (pageNo <= totalPages);
            saleItemRes.TotalResults = saleItemRes.Items.Count();
            ClearChromeConsole();
            return saleItemRes;
        }

        public async Task<TopResponse> TradeMemoAdd(string tid, string memo)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid.ToString();
            pms["memo"] = memo.ToString();
            var res = await RequestTopApi("taobao.trade.memo.add", pms);
            return res;
        }

        public async Task<TopResponse> TradeMemoUpdate(string tid, string memo,string flag)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid.ToString();
            pms["memo"] = memo.ToString();
            pms["flag"] = flag.ToString();
            //pms["reset"] = string.IsNullOrEmpty(memo).ToString();
            var res = await RequestTopApi("taobao.trade.memo.update", pms);
            return res;
        }

        public async Task<TradeFullinfoGetResponse> TradeFullinfoGet(string tid)
        {
            var pms = new Dictionary<string, string>();
            pms["fields"] = "storeId,promise_service,collect_time,delivery_time,dispatch_time,sign_time,cutoff_minutes,es_date,es_range,os_date,os_range,timing_promise,num,buyer_alipay_no,step,modified,timeout_action_time,end_time,pay_time,consign_time,rate_status,seller_nick,shipping_type,cod_status,orders.tid,orders.oid,orders.oid_str,orders.outer_iid,orders.outer_sku_id,orders.consign_time,tid,tid_str,status,end_time,buyer_nick,trade_from,credit_card_fee,buyer_rate,seller_rate,created,num,payment,pic_path,has_buyer_message,receiver_country,receiver_state,receiver_city,receiver_district,receiver_town,receiver_address,receiver_zip,receiver_name,receiver_mobile,receiver_phone,orders.timeout_action_time,orders.end_time,orders.title,orders.status,orders.price,orders.payment,orders.sku_properties_name,orders.num_iid,orders.refund_id,orders.pic_path,orders.refund_status,orders.num,orders.logistics_company,orders.invoice_no,orders.adjust_fee,seller_flag,type,post_fee,has_yfx,yfx_fee,buyer_message,buyer_flag,buyer_memo,seller_memo,orders.seller_rate,adjust_fee,invoice_name,invoice_type,invoice_kind,promotion_details,alipay_no,buyerTaxNO,pbly,orders,total_fee,orders.cid,service_orders.tmser_spu_code,step_trade_status,step_paid_fee,send_time,oaid";
            pms["tid"] = tid;
            var res = await RequestTopApi("taobao.trade.fullinfo.get", pms) as TradeFullinfoGetResponse;
            if (res.IsError && res.Trade != null)
            {
                res.Trade.TidStr = tid;
            }
            ClearChromeConsole();
            return res;
        }


        public async Task<TopResponse> TradeClose(string tid, string closeReason)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid.ToString();
            pms["close_reason"] = closeReason.ToString();
            var res = await RequestTopApi("taobao.trade.close", pms);
            return res;
        }

        public async Task<TopResponse> TradeReceivetimeDelay(string tid ,int days)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid.ToString();
            pms["days"] = days.ToString();
            var res = await RequestTopApi("taobao.trade.receivetime.delay", pms);
            return res;
        }

        public async Task<TopResponse> LogisticsCompaniesGet()
        {
            var pms = new Dictionary<string, string>();
            pms["fields"] = "id,code,name,reg_mail_no";
            var res = await RequestTopApi("taobao.logistics.companies.get", pms) as LogisticsCompaniesGetResponse;
            ClearChromeConsole();
            return res;
        }


        public async Task<TopResponse> LogisticsDummySend(string tid)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid;
            var res = await RequestTopApi("taobao.logistics.dummy.send", pms);
            return res;
        }

        public async Task<TopResponse> LogisticsOfflineSend(string tid,string companyCode,string outSid)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid;
            pms["company_code"] = companyCode;
            pms["out_sid"] = outSid;
            var res = await RequestTopApi("taobao.logistics.offline.send", pms);
            return res;
        }

        public async Task<TopResponse> LogisticsOnlineSend(string tid, string companyCode, string sid)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid;
            pms["company_code"] = companyCode;
            pms["sid"] = sid;
            var res = await RequestTopApi("taobao.logistics.online.send", pms);
            return res;
        }

        public async Task<EmployeeResponse> GetEmployees()
        {
            var cmd = @"
               imsdk.invoke('application.invokeMTopChannelService', {
                    method: 'mtop.taobao.worklink.task.group.tree',
                    param:{},
                    httpMethod: 'post',
                    version: '1.0'
                }).then(res=>console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: res})));";
            var taskId = Interlocked.Increment(ref _incrementCount);
            cmd = cmd.Replace("{api_name}", "mtop.taobao.worklink.task.group.tree")
                 .Replace("{task_command_id}", taskId.ToString());
            var res = await RequestQnApi(cmd, taskId);
            res = res.xRemoveLineBreak().xRemoveSpace();
            return Util.DeserializeNoTypeName<EmployeeResponse>(res);
        }

        public async Task<QuickPhraseResponse> GetQuickPhrases()
        {
            var cmd = @"
               QN.app.invoke({
                api: 'invokeMTopChannelService',
                query: {
                    method: 'mtop.taobao.qianniu.quickphrase.get',
                    param: {from:'',version:1.0},
                    httpMethod: 'post',
                    version: '1.0',
                },
               }).then(res=>console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: res})));";
            var taskId = Interlocked.Increment(ref _incrementCount);
            cmd = cmd.Replace("{api_name}", "mtop.taobao.qianniu.quickphrase.get")
                 .Replace("{task_command_id}", taskId.ToString());
            var res = await RequestQnApi(cmd, taskId);
            //res = res.xRemoveLineBreak().xRemoveSpace();
            var rt = Util.DeserializeNoTypeName<QuickPhraseResponse>(res);
            return rt;
        }

        public async Task<CouponResponse> GetCoupons()
        {
            var cmd = @"
               QN.app.invoke({
                api: 'invokeMTopChannelService',
                query: {
                  method: 'mtop.taobao.qianniu.airisland.coupon.get',
                  param: {buyerNick:'123456789'},
                  httpMethod: 'post',
                  version: '1.0',
                },
              }).then(res=>console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: res})));";
            var taskId = Interlocked.Increment(ref _incrementCount);
            cmd = cmd.Replace("{api_name}", "mtop.taobao.qianniu.airisland.coupon.get")
                 .Replace("{task_command_id}", taskId.ToString());
            var res = await RequestQnApi(cmd, taskId);
            res = res.xRemoveLineBreak();
            return Util.DeserializeNoTypeName<CouponResponse>(res);
        }

        public async Task<QnLoginUser> GetLoginUser()
        {
            var cmd = @"
               imsdk.invoke('application.getLoginuser')
               .then(res=>console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: res})));";
            var taskId = Interlocked.Increment(ref _incrementCount);
            cmd = cmd.Replace("{api_name}", "application.getLoginuser")
                 .Replace("{task_command_id}", taskId.ToString());
            var res = await RequestQnApi(cmd, taskId);
            res = res.xRemoveLineBreak();
            return Util.DeserializeNoTypeName<QnLoginUser>(res);
        }

        public async Task<QnConversation> GetCurrentConversationID()
        {
            var cmd = @"
               imsdk.invoke('im.uiutil.GetCurrentConversationID')
               .then(res=>console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: res})));";
            var taskId = Interlocked.Increment(ref _incrementCount);
            cmd = cmd.Replace("{api_name}", "im.uiutil.GetCurrentConversationID")
                 .Replace("{task_command_id}", taskId.ToString());
            var res = await RequestQnApi(cmd, taskId);
            res = res.xRemoveLineBreak();
            return Util.DeserializeNoTypeName<QnConversation>(res);
        }


        public async Task<List<ChatlogEntity>> GetRecentNoReplyMessages(long btime,long etime = -1)
        {
            var cmd = @"
               imsdk.invoke('im.singlemsg.ListConvByLastMsgTime', {
                    btime: '{btime}',
                    etime: '{etime}'
                }).then(async res =>{
                    var loginUser = await imsdk.invoke('im.login.GetCurrentLoginID');
                    var msgs = [];
                    var promises = res.result.map(k =>{
                        return new Promise(resolve =>{            
                            imsdk.invoke('im.singlemsg.GetLocalPageMsg', {
                                cid: {
                                    ccode: k.cid.ccode
                                },
                                count: 1,
                                gohistory: 1
                            }).then(msgRes =>{
                                var localMsg = msgRes.result.msgs[0];
                                var loginMainUser = loginUser.result.nick;
                                if(loginMainUser.indexOf(':')>-1){
                                    loginMainUser = loginMainUser.substring(0,loginMainUser.indexOf(':'))
                                }
                                var fromMainUser = localMsg.fromid.nick;
                                if(fromMainUser.indexOf(':')>-1){
                                    fromMainUser = fromMainUser.substring(0,fromMainUser.indexOf(':'))
                                }
                                var toMainUser = localMsg.toid.nick;
                                if(toMainUser.indexOf(':')>-1){
                                    toMainUser = toMainUser.substring(0,toMainUser.indexOf(':'))
                                }
                                //当前登录的千牛的主号 是否是收到的消息的主号
                                if(loginMainUser == toMainUser && loginMainUser != fromMainUser)
                                {
                                    var content = '';
                                    var imageUrl = '';
                                    if(localMsg.templateId == 101)
                                    {
                                        content = localMsg.originalData.text;
                                    }
                                    else if(localMsg.templateId == 102)
                                    {
                                        content = '客户发来一张图片';
                                        imageUrl = localMsg.originalData.url;
                                    }
                                    else if(localMsg.templateId == 107)
                                    {
                                        content = '客户发来一个文件(文件名)：'+localMsg.originalData.jsFileInfo.nodeName;
                                    }
                                    else if(localMsg.templateId == 104)
                                    {
                                        content = '客户发来一段语音';
                                    }
                                    else if(localMsg.templateId == 105)
                                    {
                                        content = '客户发来一段视频';
                                    }
                                    else if(localMsg.templateId == 106)
                                    {
                                        content = '系统消息';
                                    }
                                    else if(localMsg.templateId == 106)
                                    {
                                        content = '系统消息';
                                    }
                                    else if(localMsg.templateId == 129)
                                    {
                                        content = localMsg.summary;
                                    }
                                    else if(localMsg.templateId == 110 
                                        || localMsg.templateId == 111 || localMsg.templateId == 112
                                        || localMsg.templateId == 113 || localMsg.templateId == 114
                                        || localMsg.templateId == 120 || localMsg.templateId == 128)
                                    {
                                        content = '客户发来一条卡片消息';
                                    }
                                    else if(localMsg.templateId == 116)
                                    {
                                        content = '客户发来一条位置信息';
                                    }
                                    else if(localMsg.templateId == 152002)
                                    {
                                        content = localMsg.originalData.title;
                                    }
                                    msgs.push({
                                        FromNick:localMsg.fromid.nick,
                                        ToNick:localMsg.toid.nick,
                                        Content:content,
                                        SendTime:localMsg.sendTime,
                                        ImageUrl:imageUrl,
                                    });
                                }
                                resolve(localMsg);
                            });
                        });
                    }); 
                    Promise.all(promises).then(res=>
                        console.log('ChromeImSdkConsoleLog,'+JSON.stringify({api_name:'{api_name}',task_command_id:'{task_command_id}',sdk_response: msgs}))
                    );
                });";
            var taskId = Interlocked.Increment(ref _incrementCount);
            cmd = cmd.Replace("{api_name}", "im.singlemsg.ListConvByLastMsgTime")
                 .Replace("{task_command_id}", taskId.ToString()).Replace("{btime}",btime.ToString())
                 .Replace("{etime}",etime.ToString());
            var res = await RequestQnApi(cmd, taskId);
            res = res.xRemoveLineBreak();
            return Util.DeserializeNoTypeName<List<ChatlogEntity>>(res);
        }

        public void ClearChromeConsole()
        {
            var retVal = string.Empty;
            if (ChromOp != null)
            {
                ChromOp.Eval("console.clear()", out retVal);
            }
        }

        public Task<TopResponse> RequestTopApi(string cmd, Dictionary<string, string> pms = null)
        {
            var taskId = Interlocked.Increment(ref _incrementCount);
            var requestJs = BuildTopRequest(taskId, cmd, pms);
            var requestResetEvent = new ManualResetEventSlim(false);
            _requestWaitHandles.AddOrUpdate(taskId, requestResetEvent, (id, r) => requestResetEvent);
            return System.Threading.Tasks.Task.Run(() =>
            {
                if (ChromOp != null)
                {
                    ChromOp.Eval(requestJs);
                }
                requestResetEvent.Wait();
                TopResponse response = null;
                _responses.TryRemove(taskId, out response);
                _requestWaitHandles.TryRemove(taskId, out requestResetEvent);
                return response;
            });
        }

        public static string BuildTradeRequest(long taskId, string cmd, Dictionary<string, string> pms = null)
        {
            var paramStr = string.Empty;
            var pmList = new List<string>();
            if (pms != null && pms.Count > 0)
            {
                var idx = 0;
                foreach (var key in pms.Keys)
                {
                    var val = pms[key];
                    pmList.Add(string.Format("'{0}': '{1}'", key, val));
                    idx++;
                }
            }
            pmList.Add("'appkey': '23436601'");
            pmList.Add(string.Format("'method': '{0}'", cmd));
            paramStr = pmList.Aggregate((s1, s2) => s1 + ", " + s2);
            var sb = new StringBuilder();
            sb.Append("QN.top.invoke({"); // start
            sb.AppendFormat(" query: {{ {0} }} ", paramStr);
            sb.Append(" })");
            sb.AppendFormat(".then(res => {{ var fisrt_prop='';for(var prop in res){{fisrt_prop=prop;break}}if(res[fisrt_prop].total_results<1){{console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response:res}}))}}else{{var promises=res[fisrt_prop].trades.trade.map(trade=>{{return new Promise(resolve=>{{imsdk.invoke('im.uiutil.GetCurrentConversationID').then(buyer=>{{var buyerId=buyer.result.targetId;trade.tid_str=trade.tid.toString().substring(0,trade.tid.toString().length-4)+buyerId.toString().substring(buyerId.toString().length-2)+buyerId.toString().substring(buyerId.toString().length-4,buyerId.toString().length-2);resolve(buyer)}})}})}})}};Promise.all(promises).then(rt=>{{console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response:res}}))}})}} ,error =>{{ console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: error}})); }}) ", cmd, taskId);
            return sb.ToString();
        }

        public static string BuildTopRequest(long taskId, string cmd, Dictionary<string, string> pms = null)
        {
            var paramStr = string.Empty;
            var pmList = new List<string>();
            if (pms != null && pms.Count > 0)
            {
                var idx = 0;
                foreach (var key in pms.Keys)
                {
                    var val = pms[key];
                    pmList.Add(string.Format("'{0}': '{1}'", key, val));
                    idx++;
                }
            }
            pmList.Add("'appkey': '23436601'");
            pmList.Add(string.Format("'method': '{0}'", cmd));
            paramStr = pmList.Aggregate((s1, s2) => s1 + ", " + s2);
            var sb = new StringBuilder();
            sb.Append("QN.top.invoke({"); // start
            sb.AppendFormat(" query: {{ {0} }} ", paramStr);
            sb.Append(" })");
            sb.AppendFormat(".then(res => {{ console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: res}})); }},error =>{{ console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: error}})); }}) ", cmd, taskId);
            return sb.ToString();
        }

        public Task<string> RequestQnApi(string cmd, long taskId)
        {
            var requestResetEvent = new ManualResetEventSlim(false);
            _requestWaitHandles.AddOrUpdate(taskId, requestResetEvent, (id, r) => requestResetEvent);
            return System.Threading.Tasks.Task.Run(() =>
            {
                if (ChromOp != null)
                {
                    ChromOp.Eval(cmd);
                }
                requestResetEvent.Wait(3 * 60 * 1000);
                string response = null;
                _imsdkResponses.TryRemove(taskId, out response);
                _requestWaitHandles.TryRemove(taskId, out requestResetEvent);
                return response;
            });
        }
    }

}
