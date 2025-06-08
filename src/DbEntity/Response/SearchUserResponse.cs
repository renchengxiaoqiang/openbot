using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity.Response
{
    public class SearchUserResponse
    {
        [JsonProperty("api")]
        public string Api { get; set; }
        [JsonProperty("data")]
        public AccountData Data { get; set; }
    }


    public class AccountData
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("data")]
        public List<Account> Data { get; set; }
    }

    public class Account
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }
        [JsonProperty("encryptAccountId")]
        public string EncryptAccountId { get; set; }
        [JsonProperty("accountType")]
        public int AccountType { get; set; }
        [JsonProperty("bizType")]
        public string BizType { get; set; }
        [JsonProperty("nick")]
        public string Nick { get; set; }
        [JsonProperty("searchKey")]
        public string SearchKey { get; set; }
        [JsonProperty("searchType")]
        public string SearchType { get; set; }
    }
}
