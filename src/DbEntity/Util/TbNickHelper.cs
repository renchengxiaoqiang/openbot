using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DbEntity
{
	public class TbNickHelper
    {
        private static string _mainNickErrorPt = "[~!@#%&*+=\\s]{1}";
        private const string PubDbAccountInit = "pub_";
        private const string PrvDbAccountInit = "prv_";
        private const string ShopDbAccountInit = "shop_";

		public static bool IsMainAccount(string nick)
		{
			return !string.IsNullOrEmpty(nick) && nick.IndexOf(':') < 0 && nick.IndexOf('：') < 0;
		}

		public static string GetMainPart(string nick)
		{
			if (!string.IsNullOrEmpty(nick))
			{
				int idx = nick.IndexOf(':');
				if (idx > 0)
				{
                    nick = nick.Substring(0, idx);
				}
			}
			return nick;
		}

		public static string GetSubPart(string nick)
		{
			string rt = "";
			nick = nick.Replace('：', ':');
			int idx = nick.IndexOf(':');
			if (idx > 0)
			{
				rt = nick.Substring(idx + 1);
			}
			return rt;
		}

		public static HashSet<string> GetMainAccounts(IEnumerable<string> nicks)
		{
            var mainAccounts = new HashSet<string>();
			foreach (var nick in nicks)
			{
				string mainPart = GetMainPart(nick);
                mainAccounts.Add(mainPart);
			}
            return mainAccounts;
		}

		public static bool IsTbMainNickValid(string mainNick)
		{
			return mainNick != null && mainNick.Length > 2 && mainNick.Length < 26 && !Regex.IsMatch(mainNick, _mainNickErrorPt);
		}

		public static string ConvertNickToPubDbAccount(string nick)
		{
			return "pub_" + GetMainPart(nick);
		}

		public static string ConvertNickToShopDbAccount(string nick)
		{
			return "shop_" + GetMainPart(nick);
		}

		public static string GetWwMainNickFromShopDbAccount(string shopDba)
		{
			return shopDba.Substring("shop_".Length);
		}

		public static bool IsShopDbAccount(string shopDba)
		{
			return shopDba.StartsWith("shop_");
		}

		public static string GetWwMainNickFromPubOrPrvDbAccount(string acc)
		{
			string rt;
			if (acc.StartsWith("pub_"))
			{
				rt = acc.Substring("pub_".Length);
			}
			else
			{
				if (!acc.StartsWith("prv_"))
				{
					throw new Exception(acc + ",即非pub也非prv dbaccount");
				}
				rt = acc.Substring("prv_".Length);
			}
			return rt;
		}

		public static string GetWwMainNickFromPubDbAccount(string pubDba)
		{
			return pubDba.Substring("pub_".Length);
		}

		public static bool IsPubDbAccount(string pubDba)
		{
			return pubDba.StartsWith("pub_");
		}

		public static string ConvertNickToPrvDbAccount(string nick)
		{
			return "prv_" + nick;
		}

		public static string GetWwNickFromPrvDbAccount(string prvDba)
		{
			return prvDba.Substring("prv_".Length);
		}

		public static bool IsPrvDbAccount(string prvDba)
		{
			return prvDba.StartsWith("prv_");
		}

		public static bool IsSameShopAccount(string speaker, string talker)
		{
			return speaker == talker || GetMainPart(speaker) == GetMainPart(talker);
		}
	}
}
