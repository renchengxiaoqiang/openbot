using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.ChromeNs
{
    public class RecieveNewMessageEventArgs
    {
        public string Buyer { get; set; }
        public string Message { get; set; }
    }

    public class MessageNotifyEventArgs
    {
        public string NotifyContent;
    }

    public class BuyerSwitchedEventArgs
    {
        public LocalUser Seller;
        public Conversation Buyer;
    }
    public class SellerSwitchedEventArgs
    {
        public LocalUser Seller;
        public Conversation Buyer;
    }

    public class ShopRobotReceriveNewMessageEventArgs
    {
        public LocalUser Seller;
        public Conversation Buyer;
    }
}

