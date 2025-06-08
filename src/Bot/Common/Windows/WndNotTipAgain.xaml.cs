using BotLib;
using BotLib.Db.Sqlite;
using BotLib.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bot.Common.Windows
{
    public partial class WndNotTipAgain : EtWindow
    {
        public WndNotTipAgain()
        {
            InitializeComponent();
        }

        private WndNotTipAgain(string title, string showKey, bool showCancelButton, string okButtonText)
        {
            IsShowed = false;
            IsOkClicked = false;
            Util.Assert(!string.IsNullOrEmpty(showKey));
            InitializeComponent();
            Topmost = true;
            if (!showCancelButton)
            {
                btnCancel.Visibility = Visibility.Collapsed;
            }
            _showKey = showKey;
            if (!string.IsNullOrEmpty(title))
            {
                base.Title = title;
            }
            if (!string.IsNullOrEmpty(okButtonText))
            {
                if (!okButtonText.Contains("_"))
                {
                    okButtonText += "(_Y)";
                }
                btnOk.Content = okButtonText;
            }
        }

        private WndNotTipAgain(string text, string title, string showKey, bool showCancelButton = true, string okButtonText = null)
            : this(title, showKey, showCancelButton, okButtonText)
        {
            tblContent.Inlines.AddRange(InlineEx.ConvertTextToInlineConsiderUrl(text));
        }



        private WndNotTipAgain(List<Inline> ins, string title, string showKey, string okButtonText)
            : this(title, showKey, true, okButtonText)
        {
            tblContent.Inlines.AddRange(ins);

        }

        public static bool MyShowDialog(string message, string title = null, string showkey = null, Window owner = null, bool startUpCenterOwner = true)
        {
            var isok = true;
            DispatcherEx.xInvoke(() =>
            {
                if (!string.IsNullOrEmpty(showkey))
                {
                    if (LocalParams.GetWndNotTipAgainNeedShow(showkey))
                    {
                        var wnd = new WndNotTipAgain(message, title, showkey);
                        wnd.xSetOwner(owner);
                        wnd.xSetStartUpLocation(startUpCenterOwner);
                        var dlgRlt = wnd.ShowDialog();
                        isok = dlgRlt.HasValue && dlgRlt.Value;
                    }
                }
            });
            return isok;
        }

        public static void MyShow(string message, string title = null, string showkey = null, bool showCancelButton = true, Action<bool, bool> callback = null, Window owner = null, bool startUpCenterOwner = true, string okButtonText = null)
        {
            DispatcherEx.xInvoke(() =>
            {
                if (LocalParams.GetWndNotTipAgainNeedShow(showkey))
                {
                    var wnd = new WndNotTipAgain(message, title, showkey, showCancelButton);
                    wnd.xSetOwner(owner);
                    wnd.xSetStartUpLocation(startUpCenterOwner);
                    wnd.xShowFirstTime();
                    wnd.IsShowed = true;
                    wnd.Closed += (sender, e) =>
                    {
                        if(callback != null)
                            callback(false, wnd.IsOkClicked);
                    };

                }
                else
                {
                    if (callback != null)
                        callback(false, true);
                }
            });
        }

        public static bool MyShowDialog(List<Inline> message, string title, string showkey, Window owner = null, bool startUpCenterOwner = true, string okButtonText = null)
        {
            var isok = true;
            DispatcherEx.xInvoke(() =>
            {
                if (LocalParams.GetWndNotTipAgainNeedShow(showkey))
                {
                    var wnd = new WndNotTipAgain(message, title, showkey, okButtonText);
                    wnd.xSetOwner(owner);
                    wnd.xSetStartUpLocation(startUpCenterOwner);
                    var dlgRlt = wnd.ShowDialog();
                    isok = dlgRlt.HasValue && dlgRlt.Value;
                }
            });
            return isok;
        }

        public static void DeleteWndNotTipAgainNeedShow()
        {
            WndNotTipAgain.LocalParams.DeleteWndNotTipAgainNeedShow();
        }

        private void chkTip_Checked(object sender, RoutedEventArgs e)
        {
            WndNotTipAgain.LocalParams.SetWndNotTipAgainNeedShow(_showKey, false);
        }

        private void chkTip_UnChecked(object sender, RoutedEventArgs e)
        {
            WndNotTipAgain.LocalParams.SetWndNotTipAgainNeedShow(_showKey, true);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (WindowEx.xIsModal(this))
            {
                base.DialogResult = new bool?(true);
            }
            IsOkClicked = true;
            base.Close();
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (WindowEx.xIsModal(this))
            {
                base.DialogResult = new bool?(false);
            }
            base.Close();
        }

        public bool IsShowed;

        private string _showKey;

        public bool IsOkClicked;

        private class LocalParams
        {
            public static bool GetWndNotTipAgainNeedShow(string key)
            {
                return PersistentParams.GetParam2Key("WndNotTipAgainNeedShow", WndNotTipAgain.LocalParams.GetUniqueMasterkey(key), true);
            }

            public static void SetWndNotTipAgainNeedShow(string key, bool wndNotTipAgainNeedShow)
            {
                PersistentParams.TrySaveParam2Key("WndNotTipAgainNeedShow", WndNotTipAgain.LocalParams.GetUniqueMasterkey(key), wndNotTipAgainNeedShow);
            }

            public static void DeleteWndNotTipAgainNeedShow()
            {
                PersistentParams.Delete2KeyParamsByMasterKey("WndNotTipAgainNeedShow");
            }

            private static string GetUniqueMasterkey(string key)
            {
                if (key.Length > 10)
                {
                    key = key.Substring(0, 10) + key.GetHashCode();
                }
                return key;
            }

            private const string _masterkey = "WndNotTipAgainNeedShow";
        }
    }
}
