using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.ChatRecord
{
    public class ChatResponse
    {
        public int code { get; set; }
        public int subcode { get; set; }
        public List<ChatMessage> result { get; set; }
    }

    public class Mcode
    {
        public string clientId { get; set; }
        public string messageId { get; set; }
    }

    public class Cid
    {
        public string ccode { get; set; }
    }

    public class Fromid
    {
        public string targetType { get; set; }
        public string targetId { get; set; }
        public string nick { get; set; }
        public string display { get; set; }
        public string portrait { get; set; }
    }

    public class Toid
    {
        public string targetType { get; set; }
        public string targetId { get; set; }
        public string nick { get; set; }
        public string display { get; set; }
        public string portrait { get; set; }
    }

    public class Value
    {
        public string text { get; set; }
    }

    public class JsviewItem
    {
        public int type { get; set; }
        public Value value { get; set; }
    }

    public class OriginalData
    {
        public string url;
        public OriginalDataHeader header { get; set; }
        public string fileId { get; set; }
        /// <summary>
        /// 文字消息，可能是商品链接
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 进店卡片商品链接
        /// </summary>
        public string actionUrl { get; set; }

        public string itemId
        {
            get {
                if (!string.IsNullOrEmpty(actionUrl))
                {
                    var id = RegexEx.Match(actionUrl.ToLower(), "(?<=(item.taobao|detail.tmall).com/item.htm\\?[a-zA-Z0-9_+-./%&=]*?id=)\\d+");
                    if (!string.IsNullOrEmpty(id)) return id;
                }
                if (!string.IsNullOrEmpty(text))
                {
                    var id = RegexEx.Match(text.ToLower(), "(?<=(item.taobao|detail.tmall).com/item.htm\\?[a-zA-Z0-9_+-./%&=]*?id=)\\d+");
                    if (!string.IsNullOrEmpty(id)) return id;
                }
                return string.Empty;
            }
        }
        //public List<JsviewItem> jsview { get; set; }
    }

    public class OriginalDataHeader
    {
        public string summary { get; set; }
        public string title { get; set; }
    }


    //public class BizDataExt
    //{
    //    public string custom_paas_through_tag { get; set; }
    //    public string from_appid { get; set; }
    //    public string gws_msg_content_type { get; set; }
    //    public string msg_feature { get; set; }
    //    public string nickname { get; set; }
    //    public string ww_version { get; set; }
    //}

    public class Ext
    {
        //public BizDataExt bizDataExt { get; set; }
        public string receiver_nick { get; set; }
        public string sender_nick { get; set; }
        public long ww_msgid { get; set; }
        public string receiverMainUserId { get; set; }
    }

    //public class ExtLocal
    //{
    //    public int receiver_count { get; set; }
    //    public int unread_count { get; set; }
    //}

    //public class Unread
    //{
    //    public int count { get; set; }
    //    public string all { get; set; }
    //}

    //public class Read
    //{
    //    public int count { get; set; }
    //    public string all { get; set; }
    //}

    //public class ReceiverState
    //{
    //    public Unread unread { get; set; }
    //    public Read read { get; set; }
    //}

    //public class Remind
    //{
    //    public int remindBehavior { get; set; }
    //    public int remindType { get; set; }
    //}

    public class Loginid
    {
        public string targetType { get; set; }
        public string nick { get; set; }
        public string display { get; set; }
        public string portrait { get; set; }
        public string appkey { get; set; }
        public string targetId { get; set; }
        public string havMainId { get; set; }
    }

    public class MessageResult
    {
        public int hasMore { get; set; }

        public List<ChatMessage> msgs { get; set; }
    }

    public class ChatMessage
    {
        public int templateId { get; set; }
        public Mcode mcode { get; set; }
        public string sendTime { get; set; }
        public string sortTimeMicrosecond { get; set; }
        public Cid cid { get; set; }
        public Fromid fromid { get; set; }
        public Toid toid { get; set; }
        public string summary { get; set; }
        public int status { get; set; }
        public OriginalData originalData { get; set; }
        public Ext ext { get; set; }
        //public ExtLocal extLocal { get; set; }
        public int selfState { get; set; }
        //public ReceiverState receiverState { get; set; }
        //public string receiverReadDisable { get; set; }
        //public Remind remind { get; set; }
        public Loginid loginid { get; set; }
        public string browserid { get; set; }

        public bool IsBuyerSend { 
            get
            {
                return loginid.nick == toid.nick;
            } 
        }

        //自定义扩展属性
        public string MessageText { 
            get {
                var question = originalData.text;
                if (originalData.header != null)
                {
                    question += originalData.header.summary ?? string.Empty;
                }
                return question;
            } 
        }

        public bool HasImage
        {
            get
            {
                return IsImage(originalData.fileId);
            }
        }

        public bool IsImage(string txt)
        {
            if (string.IsNullOrEmpty(txt)) return false;
            var imgExts = new string[] { ".jpg",".jpeg",".png",".gif",".bmp" };
            return imgExts.Any(ext => txt.ToLower().EndsWith(ext));
        }

    }
}
