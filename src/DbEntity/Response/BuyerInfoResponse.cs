using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity.Response
{
    public class BuyerInfoResponse
    {
        [JsonProperty("api")]
        public string Api { get; set; }
        [JsonProperty("data")]
        public BuyerInfo Data { get; set; }
    }


    public class BuyerInfo
    {
        [JsonProperty("isShopFans")]
        public bool IsShopFans { get; set; }
        [JsonProperty("isNewCustomer")]
        public bool IsNewCustomer { get; set; }
        [JsonProperty("sendGoodRate")]
        public string SendGoodRate { get; set; }
        [JsonProperty("vipLevel")]
        public int vipLevel { get; set; }
        [JsonProperty("avgTrade")]
        public decimal? AvgTrade { get; set; }
        [JsonProperty("tradeAmount")]
        public decimal? TradeAmount { get; set; }
        [JsonProperty("tradeCount")]
        public int? TradeCount { get; set; }
        [JsonProperty("lastTradeTime")]
        public DateTime? LastTradeTime { get; set; }
    }
}
