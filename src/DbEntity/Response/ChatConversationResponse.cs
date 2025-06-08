using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class ChatConversationResponse
    {
        public string api { get; set; }
        public ChatConversationData data { get; set; }
    }

    public class ChatConversationData
    {
        public int errorCode { get; set; }
        public List<ChatConversation> result { get; set; }
    }

    public class ChatConversation
    {
        public string bizType { get; set; }
        public Cid cid { get; set; }
        public string createTime { get; set; }
        public string displayName { get; set; }
        public string modifyTime { get; set; }
        public UserId userID { get; set; }
    }

    public class Cid
    {
        public string appCid { get; set; }
        public string domain { get; set; }
    }

    public class UserId
    {
        public string appUid { get; set; }
        public string domain { get; set; }
    }
}
