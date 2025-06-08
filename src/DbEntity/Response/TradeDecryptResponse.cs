using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class TradeDecryptResponse
    {
        [JsonProperty("api")]
        public string Api { get; set; }
        [JsonProperty("data")]
        public DecryptData Data { get; set; }
    }

    public class DecryptData
    {
        [JsonProperty("result")]
        public DecryptTrade Result { get; set; }
    }

    public class DecryptTrade
    {
        [JsonProperty("addressDetail")]
        public string AddressDetail { get; set; }
        [JsonProperty("buyerNick")]
        public string BuyerNick { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("decryptDaysAfterOrderEnd")]
        public int DecryptDaysAfterOrderEnd { get; set; }
        [JsonProperty("desensitize")]
        public string Desensitize { get; set; }
        [JsonProperty("district")]
        public string District { get; set; }
        [JsonProperty("fullAddress")]
        public string FullAddress { get; set; }
        [JsonProperty("history")]
        public string History { get; set; }
        [JsonProperty("matched")]
        public string Matched { get; set; }
        [JsonProperty("mobile")]
        public string Mobile { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("orderStatus")]
        public int OrderStatus { get; set; }
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("privacyProtection")]
        public string PrivacyProtection { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("town")]
        public string Town { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("virtualNo")]
        public string VirtualNo { get; set; }
    }
}
