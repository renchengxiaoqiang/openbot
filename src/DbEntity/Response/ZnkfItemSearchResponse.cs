using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class ZnkfItemSearchResponse
    {
        public string api { get; set; }
        public ZnkfItemSearchData data { get; set; }
    }




    public class ZnkfItemSearchData
    {
        public List<ZnkfItem> data { get; set; }
    }

    public class ZnkfItem
    {
        public string title { get; set; }
        public string price { get; set; }
        public long itemId { get; set; }
        public string itemUrl { get; set; }
        public string pic { get; set; }
        public string approveStatus { get; set; }
        public int quantity { get; set; }
        public List<ZnkfItemSku> skus { get; set; }
    }



    public class ZnkfItemSkuResponse
    {
        public string api { get; set; }
        public ZnkfItemSkuData data { get; set; }
    }

    public class ZnkfItemSkuData
    {
        public string propsName { get; set; }
        public ZnkfItem item { get; set; }
    }

    public class ZnkfItemSku
    {
        public long itemId { get; set; }
        public string promotionPrice { get; set; }
        public string price { get; set; }
        public long skuId { get; set; }
        public string propsName { get; set; }
        public int quantity { get; set; }
    }
}
