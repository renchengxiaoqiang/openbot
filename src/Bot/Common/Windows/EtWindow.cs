using System;
using System.Windows;
using System.Windows.Interop;
using BotLib.Extensions;
using BotLib;
using BotLib.Wpf.Extensions;
using DbEntity;

namespace Bot.Common.Windows
{
    public partial class EtWindow : Window, System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        private string TitleTail
        {
            get
            {
                return string.IsNullOrEmpty(Seller) ? "" : string.Format(" --- {0}", Seller);
            }
        }

        private string _seller;
        public string Seller
        {
            get
            {
                return _seller;
            }
            set
            {
                _seller = value;
                base.Title = Title;
            }
        }

        public new string Title
        {
            get
            {
                return base.Title.xTrimIfEndWith(TitleTail);
            }
            set
            {
                base.Title = value.xAppendIfNotEndWith(TitleTail);
            }
        }
        public EtWindow()
        {

            Loaded += EtWindow_Loaded;
        }

        void EtWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitShowLocation();
        }

        public void ShowTip(string message, string title = null)
        {
            MsgBox.ShowTip(message, title, this, null);
        }

        private void InitShowLocation()
        {
            if (Owner != null && (ActualWidth > Owner.ActualWidth - 30.0 || ActualHeight > Owner.ActualHeight - 30.0))
            {
                Left = Owner.Left + 30.0;
                Top = Owner.Top + 30.0;
            }
        }


        public static T GetMainNickSameWindow<T>(string seller) where T : EtWindow
        {
            var appWindows = WindowEx.GetAppWindows<T>();
            foreach (T t in appWindows.xSafeForEach<T>())
            {
                if (TbNickHelper.IsSameShopAccount(t.Seller, seller))
                {
                    return t;
                }
            }
            return default(T);
        }

        public static T GetSubNickSameWindow<T>(string seller) where T : EtWindow
        {
            var appWindows = WindowEx.GetAppWindows<T>();
            foreach (T t in appWindows.xSafeForEach<T>())
            {
                if (t.Seller == seller)
                {
                    return t;
                }
            }
            return default(T);
        }

        public void FirstShow(string seller = null, Window owner = null, Action callback = null, bool startUpCenterOwner = false)
        {
            Seller = seller;
            this.xSetOwner(owner);
            this.xSetStartUpLocation(startUpCenterOwner);
            this.xKeepWindowFullVisibleAtResize();
            Closed += (sender,e) => {
                if(callback !=null) callback();
                if(Owner !=null)
                    Owner.xBrintTop();
            };
            this.xShowFirstTime();
        }

        public bool? ShowDialogEx(Window owner, string seller = null, bool startUpCenterOwner = false)
        {
            bool? dlg = null;
            try
            {
                Seller = (seller ?? Seller);
                this.xSetOwner(owner);
                this.xSetStartUpLocation(startUpCenterOwner);
                this.xKeepWindowFullVisibleAtResize();
                dlg = ShowDialog();
                if (owner != null)
                {
                    owner.xBrintTop();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return dlg;
        }

        public static T ShowOneInstance<T>(Func<T> wndFunc = null, string seller = null, Window owner = null) where T : EtWindow
        {
            T t = WindowEx.GetFirstShowingWindow<T>();
            if (t == null)
            {
                t = ((wndFunc == null) ? Activator.CreateInstance<T>() : wndFunc());
                t.FirstShow(seller, owner, null, false);
            }
            else
            {
                t.xReShow();
            }
            return t;
        }

        public static T ShowSameShopOneInstance<T>(string seller, Func<T> creater = null, Window owner = null) where T : EtWindow
        {
            T t = GetMainNickSameWindow<T>(seller);
            if (t == null)
            {
                if (creater == null)
                {
                    t = Activator.CreateInstance<T>();
                }
                else
                {
                    t = creater();
                }
                t.FirstShow(seller, owner);
            }
            else
            {
                t.xReShow();
            }
            return t;
        }

        public static T ShowSameNickOneInstance<T>(string seller, Func<T> creater = null, Window owner = null, bool startUpCenterOwner = false) where T : EtWindow
        {
            T t = GetSubNickSameWindow<T>(seller);
            if (t == null)
            {
                if (creater == null)
                {
                    t = Activator.CreateInstance<T>();
                }
                else
                {
                    t = creater();
                }
                t.FirstShow(seller, owner, null, startUpCenterOwner);
            }
            else
            {
                t.xReShow();
            }
            return t;
        }
    }
}
