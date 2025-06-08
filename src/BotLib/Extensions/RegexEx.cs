using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class RegexEx
    {
        public const string UrlHostPattern = ".*\\.(com|net|cn|org|gov|edu|cc|top|club|site|shop|wang|ltd|red|mobi|info|biz|pro|vip|ren|xin)$";
        public const string NumberCharPattern = "[0-9壹一二贰貳三参參四肆五伍六陆陸七柒八捌九玖零]";
        public const string IpAddressPattern = "(?<!\\d)((2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.){3}(2[0-4]\\d|25[0-5]|[01]?\\d\\d?)(?!\\d)";
        public const string NumberByArabPattern = "(?:[+-]?\\d{1,3}(?:,\\d{3})*(?:\\.\\d+)?(?!\\d))|(?:[+-]?\\d+(?:\\.\\d+)*)|(?:[+-]?\\.\\d+)";
        public const string EnglishWordPattern = "[a-zA-Z]+[\\-\\']?[a-zA-Z]*";
        public const string ArabNumberSequecePattern = "\\d+";
        public const string UrlPattern = "([a-zA-Z0-9_]+\\://)?([a-zA-Z0-9_-]+[.@])([a-zA-Z0-9_-]+\\.)*[a-zA-Z]+(/[a-zA-Z0-9_+-./?%&=]*)?";
        public const string GoodsPatternSmallCase = "(https?://)?(item.taobao|detail.tmall).com/item.htm\\?[a-zA-Z0-9_+-./%&=]*?id=\\d+[a-zA-Z0-9_+-./%&=]*";
        public const string GoodsUrlIdPatternSmallCase = "(?<=(item.taobao|detail.tmall).com/item.htm\\?[a-zA-Z0-9_+-./%&=]*?id=)\\d+";
        public const string EmailPattern = "[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+";
        public const string ChineseWordPattern = "[\\u4e00-\\u9fa5]+";
        public static string Match(string txt, string pattern)
        {
            if (string.IsNullOrEmpty(txt) || string.IsNullOrEmpty(pattern)) return string.Empty;
            var mch = string.Empty;
            var match = Regex.Match(txt, pattern);
            if ( match.Success)
            {
                mch = match.ToString();
            }
            return mch;
        }
    }
}
