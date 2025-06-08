using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity.Response
{
    public class MovieResponse
    {
        public int result { get; set; }
        public string msg { get; set; }
        public MovieData data { get; set; }
    }

    public class MovieData
    {
        public string film_name { get; set; }
        public int seat_num { get; set; }
        public DateTime show_time { get; set; }
        public string hall_name { get; set; }
        public string cinema_name { get; set; }
        public string cinema_address { get; set; }
        public string seat_str { get; set; }
        public int price { get; set; }
        public string orderid { get; set; }
        public string reply_msg { get; set; }
    }
}
