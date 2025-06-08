using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Bot.Automation.ChatDeskNs;
using Bot.Common;
using Bot.Common.Trivial;
using BotLib;
using BotLib.Collection;
using BotLib.Extensions;
using BotLib.Misc;
using DbEntity;
using DbEntity.Conv;
using DbEntity.Response;
using DbEntity.Trade;
using Newtonsoft.Json;
using SuperSocket.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static Bot.Params;

namespace Bot.ChatRecord
{
    public class ChatDialog
    {
        private Desk _desk;
        private ConcurrentDictionary<string, List<ZnkfItem>> _buyerInquiryItems;
        private ConcurrentBag<QNChatMessage> _waitInviteOrderBuyers;
        private ConcurrentBag<ZnkfTrade> _waitRemindPayTrades;
        //最近发送催拍的买家和时间
        private ConcurrentDictionary<string, DateTime> _inviteOrderBuyers;
        //最近发送催付的买家的订单号和时间
        private ConcurrentDictionary<string, DateTime> _remindPayTrades;
        private bool _remindPayRuning = false;
        private bool _inviteOrderRuning = false;
        private NoReEnterTimer _timerInviteOrder;
        private NoReEnterTimer _timerRemindPay;
        private SynableImageHelper _imageHelper = new SynableImageHelper(TransferFileTypeEnum.RuleAnswerImage);
        public ChatDialog(Desk desk)
        {
            _desk = desk;

            _waitInviteOrderBuyers = new ConcurrentBag<QNChatMessage>();
            _waitRemindPayTrades = new ConcurrentBag<ZnkfTrade>();
            _buyerInquiryItems = new ConcurrentDictionary<string, List<ZnkfItem>>();
            _inviteOrderBuyers = new ConcurrentDictionary<string, DateTime>();
            _remindPayTrades = new ConcurrentDictionary<string, DateTime>();
        }

    }
}

