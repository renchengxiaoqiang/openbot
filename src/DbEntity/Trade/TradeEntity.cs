using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity.Trade
{
    public class TradeEntity : EntityBase
    {
        public string BizOrderId { get; set; }
        public decimal BuyAmount { get; set; }
        public DateTime CreateTime { get; set; }
        public string OrderPrice { get; set; }
        public DateTime PayTime { get; set; }
        public string RefundFee { get; set; }
        public int SellerFlag { get; set; }
        public string SellerMemo { get; set; }
        public string TradeJson { get; set; }
    }
}
