using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class NetworkDiagnoser
    {
        public static bool IsBatAccessible
        {
            get
            {
                return BatTime.CanAccessBat;
            }
        }

        public static bool IsUrlAccessible(string url)
        {
            try
            {
                var webRequest = WebRequest.Create(url);
                var response = webRequest.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return false;
        }

        public void PingAndLogAsyn(string ipstr)
        {
            var ping = new Func<string, string>(this.Ping);
            ping.BeginInvoke(ipstr, (IAsyncResult ar) =>
            {
                string text = ping.EndInvoke(ar);
                Log.Info(text);
            }, null);
        }

        public string Ping(string ipstr)
        {
            try
            {
                if (string.IsNullOrEmpty(ipstr)) return string.Empty;
                ipstr = ipstr.Trim();
                string text = "http://";
                if (ipstr.StartsWith(text))
                {
                    ipstr = ipstr.Substring(text.Length);
                }
                Ping pingSender = new Ping();
                //Ping 选项设置  
                PingOptions options = new PingOptions();
                options.DontFragment = true;
                //测试数据  
                string data = "test data abcabc";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                //设置超时时间  
                int timeout = 1200;
                //调用同步 send 方法发送数据,将返回结果保存至PingReply实例  
                var sb = new StringBuilder();
                var reply = pingSender.Send(ipstr, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    sb.AppendLine("答复的主机地址：" + reply.Address.ToString());
                    sb.AppendLine("往返时间(ms)：" + reply.RoundtripTime);
                    sb.AppendLine("生存时间（TTL）：" + reply.Options.Ttl);
                }
                else
                {
                    sb.AppendLine(reply.Status.ToString());
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return ex.Message;
            }

        }
        
    }
}
