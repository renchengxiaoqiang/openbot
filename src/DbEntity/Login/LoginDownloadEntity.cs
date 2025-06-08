using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DbEntity
{
	public class LoginDownloadEntity
	{
		[JsonIgnore]
		public bool IsClientBanToUse
		{
			get
			{
				return !string.IsNullOrEmpty(this.ClientBanReason);
			}
		}

		public List<string> NickDatas;

        public List<string> ShopDatas;

		public string[] SynJsons;

		public string ClientBanReason;

		public string Tip;

		public UpdateDownloadEntity UpdateEntity;
	}
}
