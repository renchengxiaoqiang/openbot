using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class ZnkfTradeQueryResponse
    {
        public string api { get; set; }
        public ZnkfTradeQueryData data { get; set; }
    }


    public class ZnkfTradeQueryData
    {
        public List<ZnkfTrade> orders { get; set; }
    }

    public class ZnkfTrade
    {
        public string adjustFee { get; set; }
        public string afterSaleText { get; set; }
        public string bizOrderId { get; set; }
        public int buyAmount { get; set; }
        public string cardTypeText { get; set; }
        public string category { get; set; }
        public bool collapse { get; set; }
        public DateTime? consignTime { get; set; }
        public DateTime createTime { get; set; }
        public DateTime? endTime { get; set; }
        public string expressCompany { get; set; }
        public string expressOrderNumber { get; set; }
        public string orderPrice { get; set; }
        public DateTime? payTime { get; set; }
        public string postFee { get; set; }
        public string promotionTotalFee { get; set; }
        public string receiverAddress { get; set; }
        public string receiverMobilePhone { get; set; }
        public string receiverName { get; set; }
        public string refundFee { get; set; }
        public bool riskOrder { get; set; }
        public int sellerFlag { get; set; }
        public string sellerMemo { get; set; }
        public bool underInquiry { get; set; }
        public List<ZnkfTradeItem> itemList { get; set; }
        public List<ZnkfTradePromotion> promotionDetails { get; set; }
    }



    public class ZnkfTradeItem
    {
        public string adjustFee { get; set; }
        public string auctionId { get; set; }
        public string auctionPrice { get; set; }
        public string auctionTitle { get; set; }
        public string auctionUrl { get; set; }
        public string bizOrderId { get; set; }
        public int buyAmount { get; set; }
        public int buyerAmount { get; set; }
        public int buyerRateStatus { get; set; }
        public string cardType { get; set; }
        public string cardTypeText { get; set; }
        public DateTime createTime { get; set; }
        public DateTime? endTime { get; set; }
        public int logisticsStatus { get; set; }
        public string oldPrice { get; set; }
        public string outerId { get; set; }
        public int payStatus { get; set; }
        public DateTime? payTime { get; set; }
        public string picUrl { get; set; }
        public string price { get; set; }
        public string refundFee { get; set; }
        public int refundStatus { get; set; }
        public string refundType { get; set; }
        public string sku { get; set; }
        public string snapshotUrl { get; set; }
        public string subOrderId { get; set; }
        public bool supportPriceProtect { get; set; }
        public bool underInquiry { get; set; }
    }

    public class ZnkfTradePromotion
    {
        public string discountFee { get; set; }
        public string promotionDesc { get; set; }
        public string promotionId { get; set; }
        public string promotionName { get; set; }
    }
}
