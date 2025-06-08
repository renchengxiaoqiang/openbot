using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class ItemRecordResponse
    {
        public string api { get; set; }
        public ItemRecordData data { get; set; }
    }

    public class ItemRecordData
    {
        public string traceId { get; set; }
        public List<ZnkfItem> underInquiryItemList { get; set; }
        public List<ZnkfItem> recentlyBoughtItemList { get; set; }
        public List<ZnkfItem> footPointItemList { get; set; }
    }
}
