using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity.Conv
{
    public class AutoTaskEntity : EntityBase
    {
        public string TaskType { get; set; }
        public DateTime SendTime { get; set; }
        public string Summary { get; set; }
        public string SendUser { get; set; }
        public string RecvUser { get; set; }
        public string SendUserId { get; set; }
        public string RecvUserId { get; set; }
        public string Ccode { get; set; }
        public string Tid { get; set; }
    }
}
