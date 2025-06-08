using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class QuickPhraseResponse
    {
        public string api { get; set; }
        public QuickPhraseData data { get; set; }
    }

    public class QuickPhraseData {
        public QuickPhraseResult result { get; set; }
    }

    public class QuickPhraseResult
    {
        public List<QuickPhraseGroup> groups { get; set; }
        public List<QuickPhraseContent> list { get; set; }
    }

    public class QuickPhraseGroup
    {
        public long id { get; set; }
        public DateTime gmtCreate { get; set; }
        public DateTime gmtModified { get; set; }
        public string name { get; set; }
    }

    public class QuickPhraseContent
    {
        public long id { get; set; }
        public long groupId { get; set; }
        public DateTime gmtCreate { get; set; }
        public DateTime gmtModified { get; set; }
        public string code { get; set; }
        public string content { get; set; }
    }
}
