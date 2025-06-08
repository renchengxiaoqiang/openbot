using System;

namespace DbEntity
{
    public class AccountEntity : EntityBase
	{
        public string Nick { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LogoutTime { get; set; }
        public DateTime FisrtTime { get; set; }
	}
}
