using BotLib.BaseClass;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BotLib.Extensions;
using BotLib;
using BotLib.Misc;
using Bot.Automation.ChatDeskNs.Automators;
using System.Threading;
using Bot.AssistWindow;
using DbEntity;
using Bot.ChromeNs;
using System.Diagnostics;
using static Bot.Automation.WinApi;
using Bot.Common;
using System.IO;
using System.Security.Cryptography;
using SuperSocket.SocketEngine.Configuration;
using System.Web.UI.WebControls;
using BotLib.Db.Sqlite;
using DbEntity.Response;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Mime;
using System.Net;
using BotLib.Wpf.Extensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Bot.ChatRecord;
using Bot.AssistWindow.NotifyIcon;
using FlaUI.Core.AutomationElements;
using System.Windows.Media.Imaging;
using TextBox = FlaUI.Core.AutomationElements.TextBox;
using FlaUI.UIA3;
using Bot.AssistWindow.Widget.Robot;

namespace Bot.Automation.ChatDeskNs
{
    public class Desk : Disposable
    {
        private class DescendantHwndInfo
        {
            private int Hwnd;
            private int RootHwnd;
            private DateTime CacheTime;
            private static ConcurrentDictionary<int, DescendantHwndInfo> _cache;
            public DescendantHwndInfo(int hwnd, int rootHwnd)
            {
                CacheTime = DateTime.Now;
                Hwnd = hwnd;
                RootHwnd = rootHwnd;
            }
            private static bool GetDescendantHwndFromCache(int hWnd)
            {
                return _cache.ContainsKey(hWnd)
                    && !_cache[hWnd].CacheTime.xIsTimeElapseMoreThanMinute(1.0);
            }
            public static bool IsDescendantHwnd(int hWnd, int deskHwnd)
            {
                bool isDescendant = false; ;
                if (deskHwnd == 0) return isDescendant;
                if (GetDescendantHwndFromCache(hWnd))
                {
                    isDescendant = (_cache[hWnd].RootHwnd == deskHwnd);
                }
                else
                {
                    int rootHwnd = GetRootHwnd(hWnd);
                    _cache[hWnd] = new DescendantHwndInfo(hWnd, rootHwnd);
                    isDescendant = (rootHwnd == deskHwnd);
                }
                return isDescendant;
            }
            static DescendantHwndInfo()
            {
                _cache = new ConcurrentDictionary<int, DescendantHwndInfo>();
            }
        }
        public readonly string WndTitle;
        private DateTime _foregroundChangedTime;
        private WndAssist _assistWindow;
        private DateTime _getVisiblePercentCacheTime;
        private double _getVisiblePercentCache;
        private readonly int _processId;
        private readonly int _qnThreadId;
        private Rectangle _cacheRec;
        private List<System.Drawing.Point> _cachePoint;
        private Rectangle _restoreRect;
        private WindowState _winState;
        private bool _isForeground;
        private bool _isVisible;
        private WinEventHooker _winEventHooker;
        private NoReEnterTimer _timer;
        private object _synObj;
        private DeskEventArgs _args;

        private DateTime preUpdateRectAndWindowStateIfNeedTime;
        private DateTime _preCheckForegroundWindowTime;
        private DateTime _preUpdateLocationTime;
        private bool _isLocationChanged;
        private bool _isAlive;

        public event EventHandler<DeskEventArgs> EvLostForeground;
        public event EventHandler<DeskEventArgs> EvGetForeground;
        public event EventHandler<DeskEventArgs> EvShow;
        public event EventHandler<DeskEventArgs> EvHide;
        public event EventHandler<DeskEventArgs> EvMaximize;
        public event EventHandler<DeskEventArgs> EvMinimize;
        public event EventHandler<DeskEventArgs> EvNormalize;
        public event EventHandler<DeskEventArgs> EvMoved;
        public event EventHandler<DeskEventArgs> EvResized;
        public event EventHandler<DeskEventArgs> EvClosed;
        public event EventHandler<SellerSwitchedEventArgs> EvSellerSwitched;
        public int ProcessId
        {
            get
            {
                return _processId;
            }
        }
        public HwndInfo Hwnd
        {
            get;
            private set;
        }
        public Rectangle Rect
        {
            get;
            private set;
        }
        public Rectangle RestoreRect
        {
            get
            {
                return _restoreRect;
            }
            set
            {
                if (!value.Equals(_restoreRect))
                {
                    Rect = GetWindowRectangle(Hwnd.Handle); ;
                    Rectangle rectangle = _restoreRect;
                    _restoreRect = value;
                    if (!value.Location.Equals(rectangle.Location))
                    {
                        if (EvMoved != null)
                        {
                            EvMoved(this, _args);
                        }
                    }
                    if (!value.Size.Equals(rectangle.Size))
                    {
                        if (EvResized != null)
                        {
                            EvResized(this, _args);
                        }
                    }
                }
            }
        }
        public WindowState WinState
        {
            get
            {
                return _winState;
            }
            private set
            {
                if (value != _winState)
                {
                    Rect = GetWindowRectangle(Hwnd.Handle);
                    _winState = value;
                    switch (_winState)
                    {
                        case WindowState.Normal:
                            {
                                if (EvNormalize != null)
                                {
                                    EvNormalize(this, _args);
                                }
                                break;
                            }
                        case WindowState.Minimized:
                            {
                                if (EvMinimize != null)
                                {
                                    EvMinimize(this, _args);
                                }
                                break;
                            }
                        case WindowState.Maximized:
                            {
                                if (EvMaximize != null)
                                {
                                    EvMaximize(this, _args);
                                }
                                break;
                            }
                    }
                }
            }
        }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            private set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    if (_isVisible)
                    {
                        if (EvShow != null)
                        {
                            EvShow(this, _args);
                        }
                    }
                    else
                    {
                        if (EvHide != null)
                        {
                            EvHide(this, _args);
                        }
                    }
                }
            }
        }


        public bool IsVisibleAndNotMinimized
        {
            get
            {
                bool isVisible = WinApi.IsVisible(Hwnd.Handle);
                return isVisible && !WinApi.IsWindowMinimized(Hwnd.Handle);
            }
        }

        public bool IsForeground
        {
            get
            {
                return _isForeground;
            }
            private set
            {
                try
                {
                    if (value != _isForeground)
                    {
                        _foregroundChangedTime = DateTime.Now;
                        Util.WriteTrace("Foreground,Seller={0},visible={1}", new object[]
                        {
                            WndTitle,
                            IsVisible
                        });
                        _isForeground = value;
                        if (value)
                        {
                            if (EvGetForeground != null)
                            {
                                EvGetForeground(this, _args);
                            }
                        }
                        else
                        {
                            if (EvLostForeground != null)
                            {
                                EvLostForeground(this, _args);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }
        public WndAssist AssistWindow
        {
            get
            {
                if (_assistWindow == null)
                {
                    _assistWindow = WndAssist.AssistBag.SingleOrDefault(wnd => wnd.Desk == this);
                }
                return _assistWindow;
            }
        }
        public bool IsAlive
        {
            get
            {
                if (_isAlive)
                {
                    _isAlive = WinApi.IsHwndAlive(Hwnd.Handle);
                    if (!_isAlive)
                    {
                        base.Dispose();
                    }
                }
                return _isAlive;
            }
        }
        public bool IsMinimized
        {
            get
            {
                return IsWindowMinimized(Hwnd.Handle);
            }
        }
        public bool IsMaximized
        {
            get
            {
                return IsWindowMaximized(Hwnd.Handle);
            }
        }

        private static Desk inst;

        public static Desk Inst { get { return inst; } }



        public CtlRobot CtlRobot { get; set; }
        private Desk(QnChatWnd chatWnd)
        {
            _foregroundChangedTime = DateTime.MaxValue;
            _assistWindow = null;
            _getVisiblePercentCacheTime = DateTime.MinValue;
            _getVisiblePercentCache = 0.0;
            _isForeground = false;
            _isVisible = true;
            _synObj = new object();
            preUpdateRectAndWindowStateIfNeedTime = DateTime.MinValue;
            _preCheckForegroundWindowTime = DateTime.MinValue;
            _preUpdateLocationTime = DateTime.MinValue;
            _isLocationChanged = false;
            _isAlive = true;
            _qnThreadId = GetWindowThreadProcessId(chatWnd.Hwnd, out _processId);
            Hwnd = new HwndInfo(chatWnd.Hwnd, "ChatDesk");
            WndTitle = chatWnd.Name;
            _args = new DeskEventArgs
            {
                Desk = this
            };
            _winEventHooker = new WinEventHooker(_qnThreadId, chatWnd.Hwnd);
            _winEventHooker.EvFocused += WinEventHooker_EvFocused;
            _winEventHooker.EvLocationChanged += WinEventHooker_EvLocationChanged;
            _winEventHooker.EvTextChanged += WinEventHooker_EvTextChanged;
            WinEventHooker.EvForegroundChanged += WinEventHooker_EvForegroundChanged;
            IsForeground = IsForeground(Hwnd.Handle);
            IsVisible = IsVisible(Hwnd.Handle);
            UpdateRectAndWindowState();
            _timer = new NoReEnterTimer(Loop, 100, 0);
            inst = this;
            DelayCaller.CallAfterDelay(SetActiveQn, 3000, false);
        }

        public void ChangeBuyer(string buyer)
        {
            DispatcherEx.xInvoke(new Action(() =>
            {
                if (CtlRobot == null)
                {
                    CtlRobot = inst.AssistWindow.ctlRightPanel.GetTabItem(Bot.AssistWindow.Widget.RightPanel.TabTypeEnum.Robot).Content as CtlRobot;
                }
                CtlRobot.ChangeBuyer(buyer);
            }));
        }

        public void AddConversation(string seller, string buyer, string question, string answer,bool isAutoReply)
        {
            DispatcherEx.xInvoke(new Action(() =>
            {
                if (CtlRobot == null)
                {
                    CtlRobot = inst.AssistWindow.ctlRightPanel.GetTabItem(Bot.AssistWindow.Widget.RightPanel.TabTypeEnum.Robot).Content as CtlRobot;
                }
                CtlRobot.AddConversation(seller, buyer, question, answer, isAutoReply);
            }));
        }

        public void SetActiveQn()
        {
            //if (IsVisibleAndNotMinimized)
            //    BringTop();
            //if (automationApplication.MainWindowHandle.ToInt32() < 1) return;
            //try
            //{
            //    var topWnds = automationApplication.GetAllTopLevelWindows(uia3Automation);
            //    var mainWnd = topWnds.FirstOrDefault(k => k.ClassName == "MutilChatView");
            //    if (mainWnd == null) return;
            //    var descendants = mainWnd.FindAllDescendants();
            //    var activeTabBar = descendants.FirstOrDefault(k =>
            //    {
            //        if (k.Properties.ClassName.IsSupported && k.ClassName == "UIMutilpleTabBar")
            //        {
            //            return true;
            //        }
            //        return false;
            //    });
            //    var tabs = activeTabBar.FindAllChildren();
            //    QN qn = null;
            //    if (tabs.Length == 1)
            //    {
            //        qn = QN.QNSet.FirstOrDefault();
            //        var convRes = await qn.GetCurrentConversationID();
            //        _buyer = convRes.Result;
            //        Seller = qn.Seller;
            //    }
            //    else if (tabs.Length > 1) { }
            //    {
            //        foreach (var tab in tabs)
            //        {
            //            if (tab.Name != activeTabBar.Name)
            //            {
            //                tab.Click();
            //                break;
            //            }
            //        }
            //    }
            //}
            //catch { }
        }


        public static Desk Create(QnChatWnd wnd)
        {
            Desk desk = null;
            try
            {
                desk = new Desk(wnd);
                if (desk != null)
                {
                    DispatcherEx.xInvoke(() =>
                    {
                        WndAssist.CreateAndAttachToDesk(desk);
                    });

                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("创建desk失败,hwnd={0}", wnd.Hwnd));
            }
            return desk;
        }

        public void Hide()
        {
            if (!HideWindow(Hwnd.Handle))
            {
                WinApi.HideWindow(Hwnd.Handle);
            }
        }

        public void SetLocation(int left, int top)
        {
            Rect = new Rectangle(left, top, Rect.Width, Rect.Height);
            WinApi.SetLocation(Hwnd.Handle, left, top);
        }

        public void Show()
        {
            if (CancelHideWindow(Hwnd.Handle))
            {
                RestoreIfMinimized();
                IsVisible = true;
                if (IsAlive)
                {
                }
            }
        }

        public void RestoreIfMinimized()
        {
            if (IsMinimized)
            {
                WinApi.ShowNormal(Hwnd.Handle);
            }
        }


        private void WinEventHooker_EvForegroundChanged(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
            IsForeground = (e.Hwnd == Hwnd.Handle);
            if (!IsForeground && e.ThreadId == _qnThreadId)
            {
            }
        }

        private void WinEventHooker_EvTextChanged(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
        }

        private void WinEventHooker_EvLocationChanged(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
            lock (_synObj)
            {
                _isLocationChanged = true;
                if (QN.CurQN != null)
                {
                    QN.CurQN.Rpa.UpdateChatBrowserRect();
                }
            }
        }

        private void WinEventHooker_EvFocused(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
            if (!IsForeground && DescendantHwndInfo.IsDescendantHwnd(e.Hwnd, Hwnd.Handle))
            {
                IsForeground = true;
            }
        }

        public void ShowNormal()
        {
            WinApi.ShowNormal(Hwnd.Handle);
        }
        public void SetRect(int left, int top, int w, int h)
        {
            Rect = new Rectangle(left, top, w, h);
            WinApi.SetRect(Hwnd.Handle, left, top, w, h);
        }
        public bool IsForegroundOrVisibleMoreThanHalf(bool useCache)
        {
            return IsForeground || IsMostlyVisible(useCache);
        }
        public bool IsMostlyVisible(bool useCache = true)
        {
            return GetVisiblePercent(useCache) > 0.7;
        }
        public double GetVisiblePercent(bool useCache)
        {
            double visiblePercent;
            if (!IsVisible || WinState == WindowState.Minimized)
            {
                visiblePercent = 0.0;
            }
            else
            {
                if (!useCache || (DateTime.Now - _getVisiblePercentCacheTime).TotalMilliseconds > 10.0)
                {
                    _getVisiblePercentCache = GetVisiblePercent();
                    _getVisiblePercentCacheTime = DateTime.Now;
                }
                visiblePercent = _getVisiblePercentCache;
            }
            return visiblePercent;
        }
        private double GetVisiblePercent()
        {
            var segPts = GetDeskSegPoints();
            int visiblePtCount = segPts.Count(pt => DescendantHwndInfo.IsDescendantHwnd(WindowFromPoint(pt), Hwnd.Handle));
            return (double)visiblePtCount / (double)segPts.Count;
        }
        public bool IsBoundaryVisable()
        {
            return IsBoundaryVisable(Rect);
        }
        public bool IsBoundaryVisable(Rectangle rect)
        {
            bool isVisable = false;
            if (IsMinimized)
            {
                isVisable = false;
            }
            else
            {
                var pts = new List<System.Drawing.Point>();
                pts.Add(new System.Drawing.Point(rect.Left + 1, rect.Top + 1));
                pts.Add(new System.Drawing.Point(rect.Right - 2, rect.Top + 1));
                pts.Add(new System.Drawing.Point(rect.Left + 1, rect.Bottom - 2));
                pts.Add(new System.Drawing.Point(rect.Right - 2, rect.Bottom - 2));
                for (int i = 0; i < pts.Count; i++)
                {
                    int hWnd = WindowFromPoint(pts[i]);
                    if (!DescendantHwndInfo.IsDescendantHwnd(hWnd, Hwnd.Handle))
                    {
                        isVisable = false;
                        return isVisable;
                    }
                }
                isVisable = true;
            }
            return isVisable;
        }
        public bool CheckAlive()
        {
            return IsAlive;
        }
        private List<System.Drawing.Point> GetDeskSegPoints()
        {
            var pts = new List<System.Drawing.Point>();
            if (_cachePoint != null && _cacheRec.Equals(Rect))
            {
                pts = _cachePoint;
            }
            else
            {
                _cacheRec = Rect;
                int space = 5;
                int x = (int)((_cacheRec.Width * 0.6 - 10) / 3.0);
                int y = (_cacheRec.Height - 10) / 3;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        pts.Add(new System.Drawing.Point
                        {
                            X = space + i * x + _cacheRec.Left,
                            Y = space + j * y + _cacheRec.Top
                        });
                    }
                }
                _cachePoint = pts;
            }
            return pts;
        }
        public void BringTop()
        {
            WinApi.BringTop(Hwnd.Handle);
        }
        public bool BringTopForMs(int ms = 1000)
        {
            bool rt = false;
            DateTime now = DateTime.Now;
            while (!rt && now.xElapse().TotalMilliseconds < (double)ms)
            {
                if (WinApi.BringTop(Hwnd.Handle))
                {
                    rt = true;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
            return rt;
        }
        public void FocusEditor(bool bringtTop)
        {
        }
        public void MovedRelative(int leftDiff, int topDiff)
        {
            RestoreIfMinimized();
            UpdateRectAndWindowState();
            SetLocation(Rect.Left + leftDiff, Rect.Top + topDiff);
        }
        public void UpdateRectAndWindowState()
        {
            WindowPlacement windowPlacement = GetWindowPlacement(Hwnd.Handle);
            if (windowPlacement != null)
            {
                WinState = windowPlacement.WinState;
                Rect = ((WinState == WindowState.Maximized) ? GetWindowRectangle(Hwnd.Handle) : windowPlacement.RestoreWindow);
            }
            preUpdateRectAndWindowStateIfNeedTime = DateTime.Now;
        }
        private void Loop()
        {
            try
            {
                ListenDeskVisibleChanged();
                ListenDeskRectAndWindowStateChanged();
                ListenDeskLocatoinChanged();
                ListenDeskForegroundChanged();
            }
            catch (Exception ex)
            {
                Log.Error("ChatDesk Loop Exception=" + ex.Message);
                Log.Exception(ex);
            }
        }
        private void ListenDeskVisibleChanged()
        {
            if (_foregroundChangedTime.xIsTimeElapseMoreThanMs(50))
            {
                _foregroundChangedTime = DateTime.MaxValue;
                IsVisible = IsVisible(Hwnd.Handle);
            }
        }
        private void ListenDeskRectAndWindowStateChanged()
        {
            if (preUpdateRectAndWindowStateIfNeedTime.xIsTimeElapseMoreThanSecond(3))
            {
                UpdateRectAndWindowState();
            }
        }
        private void ListenDeskForegroundChanged()
        {
            if (_preCheckForegroundWindowTime.xIsTimeElapseMoreThanMs(300))
            {
                _preCheckForegroundWindowTime = DateTime.Now;
                IsForeground = IsForeground(Hwnd.Handle);
            }
        }
        private void ListenDeskLocatoinChanged()
        {
            if (_isLocationChanged || (DateTime.Now - _preUpdateLocationTime).TotalSeconds >= 2)
            {
                _isLocationChanged = false;
                _preUpdateLocationTime = DateTime.Now;
                WindowPlacement windowPlacement = GetWindowPlacement(Hwnd.Handle);
                if (windowPlacement != null)
                {
                    WinState = windowPlacement.WinState;
                    RestoreRect = windowPlacement.RestoreWindow;
                }
            }
        }

        protected override void CleanUp_UnManaged_Resources()
        {
            WinEventHooker.EvForegroundChanged -= WinEventHooker_EvForegroundChanged;
            if (_winEventHooker != null)
            {
                _winEventHooker.Dispose();
            }
            _winEventHooker = null;
        }

        protected override void CleanUp_Managed_Resources()
        {
            Log.Info(string.Format("ChatDesk disposed,seller={0}", WndTitle));
            _isAlive = false;
            if (_timer != null)
            {
                _timer.Dispose();
            }
            if (EvClosed != null)
            {
                EvClosed(this, new DeskEventArgs
                {
                    Desk = this
                });
            }
        }
    }

}
