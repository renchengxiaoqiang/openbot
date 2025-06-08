using System;
using BotLib;
using SuperWebSocket;
using Newtonsoft.Json;
using Bot.ChromeNs;
using Bot.Automation;
using SuperSocket.SocketBase.Config;
using Bot.AssistWindow.NotifyIcon;

namespace Bot.ChromeNs
{
    public class MyWebSocketServer
    {
        public static MyWebSocketServer WSocketSvrInst = null;
        static MyWebSocketServer()
        {
            if (WSocketSvrInst == null) WSocketSvrInst = new MyWebSocketServer();
        }

        public EventHandler<WSocketNewMessageEventArgs> OnRecieveMessage;

        public void Start()
        {
            try
            {
                var webSocket = new WebSocketServer();
                webSocket.NewSessionConnected += async (session) =>
                {
                    var cdp = new CDPClient(session);
                    var user = await cdp.GetCurrentUser();
                    var ver = await cdp.GetVersion();
                    QN qn = QN.GetByNick(user.Result);
                    qn.QnVersion = ver.version;
                    qn.CDP = cdp;
                    WndNotifyIcon.Inst.AddSellerMenuItem(qn.Seller.Nick);
                };
                webSocket.NewMessageReceived += (session, value) =>
                {
                    var wMsg = JsonConvert.DeserializeObject<WSocketMessage>(value);
                    if (wMsg.Type == "hi") return;

                    if (OnRecieveMessage != null)
                        OnRecieveMessage(session, new WSocketNewMessageEventArgs(wMsg.Type,wMsg.Response));
                };
                webSocket.SessionClosed += (session, value) =>
                {
                    Console.Write(value);
                };
                var config = new ServerConfig()
                {
                    MaxRequestLength = 5* 1024 * 1024,
                    Ip = "127.0.0.1",
                    Port = 41010
                };
                webSocket.Setup(config);//设置端口
                webSocket.Start();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            finally
            {

            }
        }
    }

    public class WSocketMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("response")]
        public string Response { get; set; }
    }

    public class WSocketNewMessageEventArgs : EventArgs
    {
        public string Type { get; private set; }
        public string Value { get; private set; }

        public WSocketNewMessageEventArgs(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }

}
