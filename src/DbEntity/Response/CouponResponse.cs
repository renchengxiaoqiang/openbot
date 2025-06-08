using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class CouponResponse
    {
        public string api { get; set; }
        public CouponData data { get; set; }
    }

    public class CouponData
    {
        public string code { get; set; }
        public List<Coupon> data { get; set; }
    }

    public class Coupon
    {
        public string activityId { get; set; }
        public string amount { get; set; }
        public string applyEndTime { get; set; }
        public string applyStartTime { get; set; }
        public int couponType { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string threshold { get; set; }
        public int personLimit { get; set; }
        public int reserveCount { get; set; }
        public int totalCount { get; set; }
    }
}
