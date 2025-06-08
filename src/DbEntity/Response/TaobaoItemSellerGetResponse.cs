using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api.Domain;

namespace DbEntity.Response
{
    public class TaobaoItemSellerGetResponse
    {
        public string api { get; set; }
        public TaobaoItemSellerData data { get; set; }
    }

    public class TaobaoItemSellerData
    {
        public Item firstResult { get; set; }
        public Item model { get; set; }
        public bool error { get; set; }
    }
}
