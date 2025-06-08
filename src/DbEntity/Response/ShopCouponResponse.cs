using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class ShopCouponResponse
    {
        public string api { get; set; }
        public AccountStatusData data { get; set; }
    }

    public class ShopCouponData
    {
        public int errorCode { get; set; }
        public List<AccountStatus> module { get; set; }
    }

    public class ShopCoupon
    {
        public long accountId { get; set; }
        public int clientSuspendStatus { get; set; }
        public long mainAccountId { get; set; }
        public int mobileClientOnlineStatus { get; set; }
        public bool mobileOnline { get; set; }
        public string nick { get; set; }
        public int pcClientExtendStatus { get; set; }
        public int pcClientOnlineStatus { get; set; }
        public bool pcOnline { get; set; }
        public bool suspend { get; set; }
    }
}
