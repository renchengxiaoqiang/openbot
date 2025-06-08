using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bot.Asset;
using Bot.Common;
using BotLib;
using BotLib.Extensions;
using DbEntity;

namespace Bot.AssistWindow.NotifyIcon.MenuCreator
{
    public class SellerMenuCreator
    {
        private static Bitmap _imgAssist;
        private static Bitmap _imgNoUse;

        static SellerMenuCreator()
        {
            _imgAssist = AssetImageHelper.GetImageFromWinFormCache(AssetImageEnum.iconYellow);
            _imgNoUse = AssetImageHelper.GetImageFromWinFormCache(AssetImageEnum.iconGray);
        }

        public static void Create(CtlNotifyIcon ctlNotifyIcon, string nick)
        {
            var it = ctlNotifyIcon.CreateItem(nick, null, null, true, nick);
            ctlNotifyIcon.InsertItem(2, it);
        }

        public static void RemoveMenuItem(CtlNotifyIcon ctlNotifyIcon, string nick)
        {
            ctlNotifyIcon.RemoveItem(nick);
        }
    }
}
