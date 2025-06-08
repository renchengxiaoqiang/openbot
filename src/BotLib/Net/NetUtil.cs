using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib.Net
{
    public static class NetUtil
    {
        private const string PortReleaseGuid = "8875BD8E-4D5B-11DE-B2F4-691756D89593";

        public static void DownFile(string url, string localfn, int len = 0)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, localfn);
            }
            if (len > 0)
            {
                Util.Assert((long)len == FileEx.GetFileLength(localfn), "下载文件大小出错");
            }
        }

        public static string ResolveServerIp(string serverDns)
        {
            try
            {
                if (serverDns.StartsWith("http://"))
                {
                    serverDns = serverDns.Substring(7);
                }
                if (serverDns.StartsWith("https://"))
                {
                    serverDns = serverDns.Substring(8);
                }
                IPAddress[] addresses = Dns.GetHostAddresses(serverDns);
                if (addresses != null && addresses.Length > 0)
                    return addresses[0].ToString();
            }
            catch (Exception ex)
            {
                Log.Error("解析dns出错，dns=" + serverDns + ",error=" + ex.Message);
            }
            return string.Empty;
        }

        public static string DowHtmlWithWebClient(string url)
        {
            return new WebClient().DownloadString(url);
        }

        public static string DownHtmlWithPost(string url, int timeoutSec = 0)
        {
            bool isok;
            string err;
            return DownHtmlWithPost(url, Encoding.UTF8, out isok, out err, 0);
        }

        public static string PostData(string url, string postStr, Encoding enc)
        {
            var webReq = WebRequest.Create(url);
            webReq.Method = "post";
            byte[] bytes = enc.GetBytes(postStr);
            var requestStream = webReq.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            var res = webReq.GetResponse();
            var reader = new StreamReader(res.GetResponseStream());
            return reader.ReadToEnd();
        }

        private static string DownHtml(string url, Encoding enc, string method, out bool isok, out string err, int timeoutSec)
        {
            isok = false;
            err = null;
            var html = string.Empty;
            if (string.IsNullOrEmpty(url)) return html;
            try
            {
                var req = WebRequest.Create(url);
                if (timeoutSec > 0)
                {
                    req.Timeout = timeoutSec;
                }
                req.Method = method;
                var res = req.GetResponse();
                var reader = new StreamReader(res.GetResponseStream());
                html = reader.ReadToEnd();
                isok = true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                Log.Error(string.Format("DownHtmlWithPost,err={0},url={1}", ex.Message, url));
            }
            return html;
        }

        public static string DownHtmlWithPost(string url, Encoding enc, out bool isok, out string err, int timeoutSec = 0)
        {
            return DownHtml(url, enc, "post", out isok, out err, timeoutSec);
        }

        public static string DownHtmlWithGet(string url, Encoding enc, int timeoutSec = 0)
        {
            bool isok;
            string err;
            return DownHtmlWithGet(url, enc, out isok, out err, timeoutSec);
        }

        public static string DownHtmlWithGet(string url, Encoding enc, out bool isok, out string err, int timeoutSec = 0)
        {
            return DownHtml(url, enc, "get", out isok, out err, timeoutSec);
        }
    }
}
