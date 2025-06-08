using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Net
{
    public class MyWebClient : WebClient
    {
        private Uri _responseUri;
        public Uri ResponseUri
        {
            get
            {
                return _responseUri;
            }
        }

        public int TimeoutMs { get; set; }

        public MyWebClient(int timeoutMs = 100000)
        {
            if (timeoutMs <= 0)
            {
                timeoutMs = 100000;
            }
            TimeoutMs = timeoutMs;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var webRes = base.GetWebResponse(request);
            _responseUri = webRes.ResponseUri;
            return webRes;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var webReq = base.GetWebRequest(address);
            if (webReq != null)
            {
                webReq.Timeout = TimeoutMs;
            }
            return webReq;
        }

        public static string DownHtml(string url, Encoding enc = null, int maxtry = 1)
        {
            string responseUrl;
            return MyWebClient.DownHtml(url, out responseUrl, 10, maxtry, false, enc);
        }

        public static string GetResponseUrl(string url)
        {
            var responseUrl = string.Empty;
            try
            {
                var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "HEAD";
                req.Timeout = 5000;
                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    responseUrl = res.ResponseUri.ToString();
                }
                res.Close();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return responseUrl;
        }


        public static string DownHtml(string url, out string responseUrl, int timeoutSec = 10, int maxTry = 1, bool setcookie = false, Encoding enc = null)
        {
            responseUrl = "";
            if ( maxTry < 1)
            {
                maxTry = 1;
            }
            if (maxTry > 5)
            {
                maxTry = 5;
            }
            if (enc == null)
            {
                enc = Util.EncodingGb2312;
            }
            var result = string.Empty;
            for (int i = 0; i < maxTry; i++)
            {
                result = "";
                var myWebClient = new MyWebClient(100000);
                try
                {
                    myWebClient.TimeoutMs = timeoutSec * 1000;
                    myWebClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    if (setcookie)
                    {
                        string cookieString = MyWebClient.GetCookieString(url);
                        myWebClient.Headers.Add("cookie", cookieString);
                    }
                    var stream = myWebClient.OpenRead(url);
                    responseUrl = myWebClient.ResponseUri.ToString();
                    var reader = new StreamReader(stream, enc);
                    result = reader.ReadToEnd();
                    break;
                }
                catch (Exception ex)
                {
                    var msg = string.Format("DownLoginedHtml,exp,nav url={0},response url={1},err={2}", url, myWebClient.ResponseUri, ex.Message);
                    Log.Error(msg);
                }
                finally
                {
                    if (myWebClient != null)
                    {
                        myWebClient.Dispose();
                    }
                }
            }
            return result;
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref int pcchCookieData, int dwFlags, object lpReserved);

        public static string GetCookieString(string url)
        {
            int len = 8192;
            var cookieStr = new StringBuilder(len);
            if (!MyWebClient.InternetGetCookieEx(url, null, cookieStr, ref len, 8192, null))
            {
                if (len < 0)
                {
                    return null;
                }
                cookieStr = new StringBuilder(len);
                if (!MyWebClient.InternetGetCookieEx(url, null, cookieStr, ref len, 8192, null))
                {
                    return null;
                }
            }
            return cookieStr.ToString();
        }

    }
}
