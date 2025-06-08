using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class MessageNotifyResponse
    {
        public string bizId { get; set; }
        public string chTopic { get; set; }
        public string ext { get; set; }
        //public NotifyExt Ext
        //{
        //    get
        //    {
        //       return JsonConvert.DeserializeObject<NotifyExt>(ext);
        //    }
        //}
        public string from_server { get; set; }
        public FromServer FromServer
        {
            get
            {
                return JsonConvert.DeserializeObject<FromServer>(from_server);
            }
        }
        public bool is_preview { get; set; }
        public string msgid { get; set; }
        public int notice_type { get; set; }
        public string open_action { get; set; }
        public string topic { get; set; }
    }

    //public class NotifyExt
    //{
    //    public string bizChainID { get; set; }
    //    public string mapNewTemplateId { get; set; }
    //    public string msg_feature { get; set; }
    //    public string pcMsgTemplateType { get; set; }
    //    public string reminder { get; set; }
    //    public string summary { get; set; }
    //    public string tag { get; set; }
    //    public string traceType { get; set; }
    //    public string vu_parent { get; set; }
    //    public string vu_real { get; set; }
    //}

    public class FromServer
    {
        [JsonProperty("notify")]
        public FromServerNotify Notify { get; set; }
    }

    public class FromServerNotify
    {
        public string id { get; set; }
        public string biz_id { get; set; }
        public string chs_topic { get; set; }
        public string topic { get; set; }
        public string status { get; set; }

        [JsonProperty("alert")]
        public NotifyAlert Alert { get; set; }
        public string modified { get; set; }
        public string chs_status { get; set; }
        private string biz { get; set; }

        //public NotifyBiz Biz
        //{
        //    get
        //    {
        //        return JsonConvert.DeserializeObject<NotifyBiz>(biz);

        //    }
        //}

        public List<string> content { get; set; }
        public string tpnMsgid { get; set; }
        private string moreContent { get; set; }
        //public NotifyMoreContent MoreContent
        //{
        //    get
        //    {
        //        if (moreContent == null)
        //        {
        //            return null;
        //        }
        //        return JsonConvert.DeserializeObject<NotifyMoreContent>(moreContent);
        //    }
        //}

        public int total { get; set; }
        public string accountOriId { get; set; }
        public string extend { get; set; }

        public class NotifyAlert
        {
            public string picture { get; set; }
            public string pict { get; set; }
            public string buyer_nick { get; set; }
            public string title { get; set; }
            public string buyer_UID { get; set; }
        }

        public class NotifyBiz
        {
            public string buyer_nick { get; set; }
            public string _task_biz_id_ { get; set; }
            public string tid { get; set; }
            public string refundId { get; set; }
        }


        public class NotifyMoreContent
        {
            public string buyer_nick { get; set; }
            public string buyer_id { get; set; }
        }

        //public class TradeNotifyMoreContent
        //{
        //    public string buyer_nick { get; set; }
        //    public string post_fee { get; set; }
        //    public int num { get; set; }
        //    public string payment { get; set; }
        //    public string buyer_id { get; set; }
        //    public string title { get; set; }
        //    public string contact_id { get; set; }
        //    public string sku_prop { get; set; }
        //    public string tid { get; set; }
        //    public string picture { get; set; }
        //    public bool sendImba { get; set; }
        //}

        //public class RefundNotifyMoreContent
        //{
        //    public string buyer_nick { get; set; }
        //    public string reason { get; set; }
        //    public string refund_fee { get; set; }
        //    public string oid { get; set; }
        //    public string noFilter { get; set; }
        //    public string refundId { get; set; }
        //    public string title { get; set; }
        //    public string buyer_id { get; set; }
        //    public bool sendImba { get; set; }
        //}
    }

}
