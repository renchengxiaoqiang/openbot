using Bot.Common.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BotLib.Wpf.Extensions;

namespace Bot.Common
{
    public class MsgBox
    {
        public static void ShowNotTipAgain(string msg, string title, string showKey, Window owner = null)
        {
            WndNotTipAgain.MyShow(msg, title, showKey, false, null, owner, true);
        }
        public static void ShowNotTipAgain(string msg, string title, string showKey, Action<bool, bool> cb = null, Window owner = null, string okButtonText = null)
        {
            WndNotTipAgain.MyShow(msg, title, showKey, true, cb, owner, true, okButtonText);
        }
        public static bool ShowNotTipDialog(string msg, string title, string showKey, Window owner = null)
        {
            return WndNotTipAgain.MyShowDialog(msg, title, showKey, owner, true);
        }
        public static string ShowInputDialog(string tip, string title, string inputDef = null, string helpUrl = null, Func<string, string> validator = null, Window owner = null)
        {
            return WndInput.MyShowDialog(tip, title, inputDef, helpUrl, validator, owner, false);
        }
        public static void ShowErrDialog(string msg, DependencyObject ownerDescendant = null)
        {
            MsgBox.ShowDialog(msg, ownerDescendant, "错误提示");
        }
        public static void ShowDialog(string msg, DependencyObject ownerDescendant = null, string title = null)
        {
            MsgBox.ShowDialogEx(msg, title, ownerDescendant.xFindParentWindow(), false, false, null, null);
        }
        public static bool ShowDialogEx(string message, string title = null, DependencyObject ownerDescendant = null, string yesText = null, string noTxt = null)
        {
            bool? rt = MsgBox.ShowDialogEx(message, title, (ownerDescendant != null) ? ownerDescendant.xFindParentWindow() : null, true, false, yesText, noTxt);
            return rt.HasValue && rt.Value;
        }
        public static void ShowTip(string msg, Action<bool> callback = null, string title = null, DependencyObject ownerDescendant = null)
        {
            MsgBox.ShowTip(msg, title, ownerDescendant.xFindParentWindow(), true, callback, false);
        }
        public static void ShowTip(string message, string title = "提示", DependencyObject ownerDescendant = null, Action callback = null)
		{
            MsgBox.ShowTip(message, title, ownerDescendant.xFindParentWindow(), false, (isYesButtonClicked) =>
            { if (isYesButtonClicked && callback!=null) callback(); }, false);
		}
        public static void ShowErrTip(string msg, DependencyObject ownerDescendant = null)
        {
            MsgBox.ShowTip(msg, "错误提示", ownerDescendant, null);
        }
        public static void ShowTrayTip(string msg, string title = null, int showSecond = 30, string closeKey = null, Action cb = null)
        {
            WndTrayTip.ShowTrayTip(msg, title, showSecond, closeKey, cb);
        }
        public static void CloseTrayTip(string closeKey)
        {
            WndTrayTip.Close(closeKey);
        }
        public static void ShowTrayTip(string msg, string title = null)
        {
            WndTrayTip.ShowTrayTip(msg, title);
        }
        private static bool? ShowDialogEx(string message, string title, Window owner, bool showCancelButton, bool startUpCenterOwner = false, string yesText = null, string noText = null)
		{
			bool? rt = null;
			DispatcherEx.xInvoke(()=>{
                var wnd = new WndMsgBox(message,title,showCancelButton);
                if(!string.IsNullOrEmpty(noText))
                {
                    wnd.btnCancel.Content = noText;
                }
                if(!string.IsNullOrEmpty(yesText))
                {
                    wnd.btnOk.Content = yesText;
                }
                wnd.Topmost = true;
                wnd.xSetOwner(owner);
                wnd.xSetStartUpLocation(startUpCenterOwner);
                rt = wnd.ShowDialog();
            });
			return rt;
		}
        private static void ShowTip(string message, string title, Window owner, bool showCancelButton, Action<bool> callback, bool startUpCenterOwner = false)
		{
			DispatcherEx.xInvoke(()=>{
                var wnd = new WndMsgBox(message,title,showCancelButton,callback);
                wnd.xSetOwner(owner);
                wnd.xSetStartUpLocation(startUpCenterOwner);
                wnd.xShowFirstTime();
            });
		}
        private static void SetOwner(Window wind, Window owner)
        {
            if (owner != null)
            {
                if (owner != wind)
                {
                    wind.Owner = owner;
                }
            }
            else
            {
                owner = Application.Current.MainWindow;
                if (owner != wind)
                {
                    wind.Owner = Application.Current.MainWindow;
                }
            }
        }
    }
}
