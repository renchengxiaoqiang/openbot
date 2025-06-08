using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Bot.AssistWindow.Widget;
using Bot.Automation;
using Bot.Automation.ChatDeskNs;
using Bot.ChromeNs;
using BotLib;
using BotLib.Db.Sqlite;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;

namespace Bot.AssistWindow
{
    public partial class WndAssist : Window
    {
        public static ConcurrentBag<WndAssist> AssistBag;
        public readonly Desk Desk;
        private static NoReEnterTimer _timer;
        public const bool TrackAsOwnedWindow = false;
        private DelayCaller _foregroundTrackDelayCaller;
        private int _handle;
        public int Handle
        {
            get
            {
                if (_handle == 0)
                {
                    _handle = this.xHandle();
                }
                return _handle;
            }
        }

        public bool IsWakeUp;
        private ConcurrentBag<int> _closingHwndBag;
        private DateTime _prePopTime;
        private static DateTime _preKeepMultiWndAssitZOrderRightIfNeedTime;
        private bool _isTracking;
        private bool _isFirstTrack;
        private XYRatio _ratio;
        private double _deskPreWidth;
        private double _deskPreHeight;
        public bool IsShowRightPanel;
        private DateTime _preMouseDownTime;
        public bool IsClosed { get; set; }

        public double ToLogicalWidth()
        {
            return ToLogicalRatio.XRatio * (double)Desk.Rect.Width;
        }

        public double ToLogicalHeight()
        {
            return ToLogicalRatio.YRatio * (double)Desk.Rect.Height;
        }

        public XYRatio ToLogicalRatio
        {
            get
            {
                System.Windows.Point point = PointFromScreen(new System.Windows.Point(100.0, 100.0));
                return new XYRatio
                {
                    XRatio = point.X / 100.0,
                    YRatio = point.Y / 100.0
                };
            }
        }

        public XYRatio ToLogicalRatioWithUIThread
        {
            get
            {
                XYRatio rt = new XYRatio();
                DispatcherEx.xInvoke(() =>
                {
                    System.Windows.Point point = PointFromScreen(new System.Windows.Point(100.0, 100.0));
                    rt.XRatio = point.X / 100.0;
                    rt.YRatio = point.Y / 100.0;
                });
                return rt;
            }
        }

        public static class WaParams
        {
            public const int DeskPanelSpace = 6;
            public const int RightPanelMinWidth = 365;
            public const int RightPanelDefaultWidth = 400;
            public const int BottomPanelMinHeight = 100;
            public const int BottomPanelDefaultHeight = 150;

            public static int GetRightPanelWidth(string seller)
            {
                return PersistentParams.GetParam2Key("PanelRightWidth", seller, 400);
            }

            public static void SetRightPanelWidth(string seller, int width)
            {
                Util.Assert(width >= 365);
                PersistentParams.TrySaveParam2Key("PanelRightWidth", seller, width);
            }

            public static int GetBottomPanelHeight(string seller)
            {
                return PersistentParams.GetParam2Key("PanelBottomHeight", seller, 150);
            }

            public static void SetBottomPanelHeight(string seller, int height)
            {
                Util.Assert(height >= 100);
                PersistentParams.TrySaveParam2Key("PanelBottomHeight", seller, height);
            }

        }

        private class TopWnd
        {
            public Desk TopDesk;
            public WndAssist TopAssistWnd;
            public int DeskZorder;
            public int WndZorder;
            public bool HasOtherWndAssitUptoTopDesk;
        }

        public class XYRatio
        {
            public double XRatio;
            public double YRatio;
        }

        private enum MoveTypeEnum
        {
            Unknown,
            LeftTop,
            RightTop
        }
        static WndAssist()
        {
            AssistBag = new ConcurrentBag<WndAssist>();
            _preKeepMultiWndAssitZOrderRightIfNeedTime = DateTime.MinValue;
            _timer = new NoReEnterTimer(LoopForTrack, 1500, 0);
        }

        public WndAssist(Desk desk)
        {
            _handle = 0;
            IsWakeUp = false;
            _closingHwndBag = new ConcurrentBag<int>();
            _prePopTime = DateTime.MinValue;
            _isTracking = false;
            _isFirstTrack = true;
            _ratio = null;
            IsShowRightPanel = true;
            _preMouseDownTime = DateTime.MinValue;
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                try
                {
                    try
                    {
                        var text = "微软雅黑";
                        if (IsFontFamilyExist(text))
                        {
                            base.FontFamily = new System.Windows.Media.FontFamily(text);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }
                    Desk = desk;
                    this.xHideToAltTab();
                    Left = 0.0;
                    Top = 0.0;
                    Width = SystemParameters.VirtualScreenWidth;
                    Height = SystemParameters.WorkArea.Height;
                    InitDeskEvent();
                    Closed += WndAssist_Closed;
                    HiddenControls();
                    InitControls();
                    Loaded += WndAssist_Loaded;
                }
                catch (Exception e2)
                {
                    Log.Exception(e2);
                }
            }
        }

        private void InitControls()
        {
            ctlRightPanel.Init(this);
        }

        private void HiddenControls()
        {
            grdQnTab.Visibility = Visibility.Hidden;
            ctlRightPanel.Visibility = Visibility.Hidden;
        }

        private void InitDeskEvent()
        {
            RemoveDeskEvent();
            AddDeskEvent();
        }

        private void AddDeskEvent()
        {
            Desk.EvShow += Desk_EvShow;
            Desk.EvNormalize += Desk_EvNormalize;
            Desk.EvHide += Desk_EvHide;
            Desk.EvClosed += Desk_EvClosed;
            Desk.EvMaximize += Desk_EvMaximize;
            Desk.EvMinimize += Desk_EvMinimize;
            Desk.EvMoved += Desk_EvMoved;
            Desk.EvResized += Desk_EvResized;
            Desk.EvGetForeground += Desk_EvGetForeground;
            Desk.EvLostForeground += Desk_EvLostForeground;
            Desk.EvSellerSwitched += Desk_EvSellerSwitched;
        }

        private void RemoveDeskEvent()
        {
            Desk.EvShow -= Desk_EvShow;
            Desk.EvNormalize -= Desk_EvNormalize;
            Desk.EvHide -= Desk_EvHide;
            Desk.EvClosed -= Desk_EvClosed;
            Desk.EvMaximize -= Desk_EvMaximize;
            Desk.EvMinimize -= Desk_EvMinimize;
            Desk.EvMoved -= Desk_EvMoved;
            Desk.EvResized -= Desk_EvResized;
            Desk.EvGetForeground -= Desk_EvGetForeground;
            Desk.EvLostForeground -= Desk_EvLostForeground;
            Desk.EvSellerSwitched -= Desk_EvSellerSwitched;
        }

        private void Desk_EvSellerSwitched(object sender, SellerSwitchedEventArgs e)
        {

        }

        private void Desk_EvLostForeground(object sender, DeskEventArgs e)
        {
        }

        private void Desk_EvGetForeground(object sender, DeskEventArgs e)
        {
            DispatcherEx.xInvoke(() =>
            {
                ShowAssist();
                if (_foregroundTrackDelayCaller != null)
                {
                    _foregroundTrackDelayCaller.CallAfterDelay();
                }
            });
        }

        private void Desk_EvResized(object sender, DeskEventArgs e)
        {
            Track(false, false);
        }

        private void Desk_EvMoved(object sender, DeskEventArgs e)
        {
            Track(false, true);
        }

        private void Desk_EvMinimize(object sender, DeskEventArgs e)
        {
            DispatcherEx.xInvoke(() =>
            {
                try
                {
                    Hide();
                    Sleep();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            });
        }

        private void Desk_EvMaximize(object sender, DeskEventArgs e)
        {
            Desk.ShowNormal();
            DispatcherEx.xInvoke(() =>
            {
                double maxW = SystemParameters.WorkArea.Width - WaParams.GetRightPanelWidth(Desk.WndTitle) - (double)12f;
                int w = (int)(maxW / ToLogicalRatio.XRatio);
                double maxH = SystemParameters.WorkArea.Height - (double)12f;
                int h = (int)(maxH / ToLogicalRatio.YRatio);
                Desk.SetRect(6, 6, w, h);
            });
            Track(false, false);
        }

        private void Desk_EvClosed(object sender, DeskEventArgs e)
        {
            DispatcherEx.xInvoke(() =>
            {
                Close();
            });
        }

        private void Desk_EvHide(object sender, DeskEventArgs e)
        {
            DispatcherEx.xInvoke(() =>
            {
                Hide();
                Sleep();
            });
        }

        private void Sleep()
        {
            if (IsWakeUp)
            {
                IsWakeUp = false;
                DispatcherEx.xInvoke(() =>
                {
                    ctlRightPanel.Sleep();
                });
            }
        }

        private void Desk_EvNormalize(object sender, DeskEventArgs e)
        {
            WakeUp();
            Track(false, false);
        }

        private void WakeUp()
        {
            if (!IsWakeUp)
            {
                IsWakeUp = true;
                DispatcherEx.xInvoke(() =>
                {
                    ctlRightPanel.WakeUp();
                });
            }
        }

        private void Desk_EvShow(object sender, DeskEventArgs e)
        {
            DispatcherEx.xInvoke(() =>
            {
                if (!Desk.IsMinimized && Desk.IsVisible && base.Visibility > Visibility.Visible)
                {
                    ShowAssist();
                }
            });
        }

        private static void AddToCache(WndAssist wndAssist)
        {
            if (!AssistBag.Contains(wndAssist))
            {
                AssistBag = new ConcurrentBag<WndAssist>(AssistBag)
				{
					wndAssist
				};
            }
        }

        private static void ReomveFromCache(WndAssist wndAssist)
        {
            if (AssistBag.Contains(wndAssist))
            {
                AssistBag = AssistBag.xRemove(wndAssist);
            }
        }

        private void WndAssist_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= WndAssist_Loaded;
            AddToCache(this);
            _foregroundTrackDelayCaller = new DelayCaller(() =>
            {
                if (Desk.IsMostlyVisible(false))
                {
                    Track(false, false);
                }
            }, 100, true);
        }

        private void WndAssist_Closed(object sender, EventArgs e)
        {
            RemoveDeskEvent();
            ReomveFromCache(this);
        }

        private bool IsFontFamilyExist(string fontName)
        {
            bool f = false;
            try
            {
                using (Font font = new Font(fontName, 12f))
                {
                    f = (font.Name == fontName);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return f;
        }

        private static void LoopForTrack()
        {
            try
            {
                foreach (WndAssist wndAssist in AssistBag)
                {
                    wndAssist.Track(false, true);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _preMouseDownTime).TotalSeconds > 1.0)
            {
                _preMouseDownTime = DateTime.Now;
                Desk.BringTop();
            }
        }

        public void SetOwner()
        {
        }

        private void btnShowRight_Click(object sender, RoutedEventArgs e)
        {
            ShowPanelRight();
        }

        public void ShowPanelRight()
        { 
            IsShowRightPanel = true;
            btnShowRight.Visibility = Visibility.Hidden;
            ctlRightPanel.Visibility = Visibility.Visible;
            Track(false, false);
        }

        public void HidePanelRight()
        {
            IsShowRightPanel = false;
            btnShowRight.Visibility = Visibility.Visible;
            ctlRightPanel.Visibility = Visibility.Hidden;
            Track(false, false);
        }

        public void BringTop()
        {
            try
            {
                if (IsLoaded)
                {
                    Topmost = true;
                    DispatcherEx.DoEvents();
                    Topmost = false;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void CreateAndAttachToDesk(Desk desk)
        {
            var wndAssist = new WndAssist(desk);
            wndAssist.AttachToDesk();
        }

        public void AttachToDesk()
        {
            if (Desk.IsForegroundOrVisibleMoreThanHalf(true))
            {
                ShowAssist();
            }
            else
            {
                Desk.EvGetForeground -= Desk_EvGetForeground;
                Desk.EvGetForeground += Desk_EvGetForeground;
                Desk.EvNormalize -= Desk_EvNormalize;
                Desk.EvNormalize += Desk_EvNormalize;
            }
        }

        private void ShowAssist()
        {
            Show();
            Track(false, false);
            WakeUpAssist();
        }

        private void WakeUpAssist()
        {
            if (!IsWakeUp)
            {
                IsWakeUp = true;
                DispatcherEx.xInvoke(() =>
                {
                    ctlRightPanel.WakeUp();
                });
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        private void Track(bool adjustDeskLocation = false, bool isLoopTrack = false)
        {
            if (!_isTracking && !IsClosed)
            {
                _isTracking = true;
                DispatcherEx.xInvoke(() =>
                {
                    try
                    {
                        if (!IsLoaded || IsHidden())
                        {
                            Hide();
                        }
                        else
                        {
                            if (!IsVisible && Desk.GetVisiblePercent(true) > 0.0)
                            {
                                Show();
                            }
                            if (IsVisible)
                            {
                                SetPanelsSize();
                                if (_isFirstTrack | adjustDeskLocation)
                                {
                                    _isFirstTrack = false;
                                    SetDeskLocation();
                                }

                                SetRightPanelPosition();
                                if (!isLoopTrack && Desk.IsMostlyVisible(true))
                                {
                                    BringTop();
                                }
                                if (isLoopTrack)
                                {
                                    BringWndAssitOfTopDeskTopMostIfNeed();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                });
                _isTracking = false;
            }
        }

        private static void BringWndAssitOfTopDeskTopMostIfNeed()
        {
            try
            {
                if ((DateTime.Now - _preKeepMultiWndAssitZOrderRightIfNeedTime).TotalMilliseconds >= 100.0)
                {
                    _preKeepMultiWndAssitZOrderRightIfNeedTime = DateTime.Now;
                    TopWnd topWnd = null;
                    var vwset = new List<WndAssist>();
                    while (true)
                    {
                        vwset = AssistBag.Where(k=>k.IsVisible).ToList();
                        if (vwset.Count <= 1)
                        {
                            if (vwset.Count == 1)
                            {
                                var wnd = vwset.First();
                                if (wnd.Desk.GetVisiblePercent(true) == 0.0)
                                {
                                    wnd.Hide();
                                }
                            }
                            break;
                        }
                        topWnd = GetTopMostAssistWindowHwnd(vwset);
                        if (topWnd == null || topWnd.TopAssistWnd == null || topWnd.TopDesk == null)
                        {
                            break;
                        }
                        if (topWnd.TopDesk.GetVisiblePercent(true) != 0.0)
                        {
                            if (((topWnd.TopAssistWnd != null) ? topWnd.TopAssistWnd.Desk : null) != topWnd.TopDesk || topWnd.HasOtherWndAssitUptoTopDesk)
                            {
                                DelayCaller.CallAfterDelayInUIThread(() =>
                                {
                                    BrindTopWndAssistAndDesk(topWnd);
                                }, 500);
                            }
                            break;
                        }
                        topWnd.TopAssistWnd.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private static void BrindTopWndAssistAndDesk(TopWnd wnd)
        {
            var vwset = AssistBag.Where(k => k.IsVisible).ToList();
            if (vwset.Count > 1)
            {
                var topWnd = GetTopMostAssistWindowHwnd(vwset);
                bool hasOtherTopDesk;
                if (topWnd.TopDesk == wnd.TopDesk)
                {
                    hasOtherTopDesk = (((topWnd.TopAssistWnd != null) ? topWnd.TopAssistWnd.Desk : null) != topWnd.TopDesk || topWnd.HasOtherWndAssitUptoTopDesk);
                }
                else
                {
                    hasOtherTopDesk = false;
                }
                if (hasOtherTopDesk)
                {
                    var topWndAssist = vwset.FirstOrDefault(k => k.Desk == topWnd.TopDesk);
                    if (topWndAssist != null && topWndAssist.Desk.IsMostlyVisible(true))
                    {
                        topWndAssist.BringTop();
                        topWndAssist.Desk.BringTop();
                    }
                }
            }
        }

        private static TopWnd GetTopMostAssistWindowHwnd(List<WndAssist> wnds)
        {
            var wndHwnds = wnds.Select(k => k.Handle);
            var deskHwnds = wnds.Select(k => k.Desk.Hwnd.Handle);
            var wndZorderDict = new Dictionary<int, int>();
            var topDeskHwnd = 0;
            var topWndHwnd = 0;
            var deskZorder = 0;
            var wndZorder = 0;
            var zorder = int.MaxValue;
            WinApi.TraverDesktopHwnd((hwnd) =>
            {
                int fzorder = zorder;
                zorder = fzorder - 1;
                if (wndHwnds.Contains(hwnd))
                {
                    if (topWndHwnd == 0)
                    {
                        topWndHwnd = hwnd;
                        wndZorder = zorder;
                    }
                    wndZorderDict[hwnd] = zorder;
                }
                else if (deskHwnds.Contains(hwnd) && topDeskHwnd == 0)
                {
                    topDeskHwnd = hwnd;
                    deskZorder = zorder;
                }
                return topWndHwnd == 0 || topDeskHwnd == 0;
            }, null, null);
            var wndAssist = wnds.FirstOrDefault(k => k.Desk.Hwnd.Handle == topDeskHwnd);
            var topDesk = wndAssist != null ? wndAssist.Desk : null;
            bool hasOtherWndAssitUptoTopDesk = false;
            if (topDesk != null)
            {
                var wndAssistOfTop = wnds.FirstOrDefault(k => k.Desk == topDesk);
                var assistWndHandleOfTopdesk = ((wndAssistOfTop != null) ? new int?(wndAssistOfTop.Handle) : null);
                if (assistWndHandleOfTopdesk != null)
                {
                    hasOtherWndAssitUptoTopDesk = wndZorderDict.Where(kv =>
                    {
                        bool rt;
                        if (kv.Value > topDeskHwnd)
                        {
                            rt = !(kv.Key == assistWndHandleOfTopdesk.GetValueOrDefault() & assistWndHandleOfTopdesk != null);
                        }
                        else
                        {
                            rt = false;
                        }
                        return rt;
                    }).Any();
                }
            }
            var topWnd = new TopWnd();
            var topWndAssist = wnds.FirstOrDefault(k => k.Desk.Hwnd.Handle == topDeskHwnd);
            topWnd.TopDesk = ((topWndAssist != null) ? topWndAssist.Desk : null);
            topWnd.TopAssistWnd = wnds.FirstOrDefault(k => k.Handle == topWndHwnd);
            topWnd.DeskZorder = deskZorder;
            topWnd.WndZorder = wndZorder;
            topWnd.HasOtherWndAssitUptoTopDesk = hasOtherWndAssitUptoTopDesk;
            return topWnd;
        }

        private bool IsHidden()
        {
            return !Desk.IsVisible || Desk.IsMinimized || (Desk.Rect.X < -1000 && Desk.Rect.Y < -100) || Desk.Rect.Width <= 0;
        }

        private void SetPanelsSize()
        {
            double deskLogicalHeight = GetDeskLogicalHeight();
            double deskLogicalWidth = GetDeskLogicalWidth();
            if (deskLogicalHeight != _deskPreHeight || deskLogicalWidth != _deskPreWidth)
            {
                double rw = GetRightPanelWidth(deskLogicalWidth);
                FrameworkElementEx.xSetWidth(ctlRightPanel, rw);
                int rh = (int)deskLogicalHeight;
                FrameworkElementEx.xSetHeight(ctlRightPanel, (double)rh);
                _deskPreHeight = deskLogicalHeight;
                _deskPreWidth = deskLogicalWidth;
            }
        }

        public double GetDeskLogicalWidth()
        {
            return ToLogicalRatio.XRatio * (double)Desk.Rect.Width;
        }

        public double GetDeskLogicalHeight()
        {
            return ToLogicalRatio.YRatio * (double)Desk.Rect.Height;
        }

        private double GetBottomPanelHeight(double deskLogicalHeight)
        {
            double bHeight = (double)WaParams.GetBottomPanelHeight(Desk.WndTitle);
            double fullHeight = deskLogicalHeight + 12.0 + bHeight;
            double val = SystemParameters.WorkArea.Height - deskLogicalHeight + 12.0;
            if (fullHeight > SystemParameters.WorkArea.Height)
            {
                bHeight = Math.Max(val, 100.0);
                WaParams.SetBottomPanelHeight(Desk.WndTitle, (int)bHeight);
            }
            else if (bHeight < 150.0)
            {
                bHeight = Math.Min(val, 150.0);
                WaParams.SetBottomPanelHeight(Desk.WndTitle, (int)bHeight);
            }
            return bHeight;
        }

        private double GetRightPanelWidth(double deskLogicalWidth)
        {
            double rpWidth = (double)WaParams.GetRightPanelWidth(Desk.WndTitle);
            double fullWidth = deskLogicalWidth + 12.0 + rpWidth;
            double val = SystemParameters.WorkArea.Width - deskLogicalWidth + 12.0;
            if (fullWidth > SystemParameters.WorkArea.Width)
            {
                rpWidth = Math.Max(val, 365.0);
                WaParams.SetRightPanelWidth(Desk.WndTitle, (int)rpWidth);
            }
            else if (rpWidth < 400.0)
            {
                rpWidth = Math.Min(val, 400.0);
                WaParams.SetRightPanelWidth(Desk.WndTitle, (int)rpWidth);
            }
            return rpWidth;
        }

        public double GetDeskLogicalLeft()
        {
            return base.PointFromScreen(new System.Windows.Point((double)Desk.Rect.Left, 0.0)).X;
        }

        public double GetDeskLogicalTop()
        {
            return base.PointFromScreen(new System.Windows.Point(0.0, (double)Desk.Rect.Top)).Y;
        }

        private void SetDeskLocation()
        {
            int minLeft = 0;
            int minTop = 0;
            double deskLogicalWidth = GetDeskLogicalWidth();
            double rpWidth = (double)WaParams.GetRightPanelWidth(Desk.WndTitle);
            double deskLogicalLeft = GetDeskLogicalLeft();
            if (deskLogicalLeft < 6.0 || deskLogicalLeft + deskLogicalWidth + rpWidth + 6.0 > SystemParameters.WorkArea.Width)
            {
                minLeft =(int)(SystemParameters.WorkArea.Width - deskLogicalWidth - rpWidth - 12.0);
                if (minLeft < 6)
                {
                    minLeft = 6;
                }
            }
            double deskLogicalHeight = GetDeskLogicalHeight();
            double bpHeight = (double)WaParams.GetBottomPanelHeight(Desk.WndTitle);
            double deskLogicalTop = GetDeskLogicalTop();
            if (deskLogicalTop < 6.0 || deskLogicalTop + deskLogicalHeight + bpHeight + 12.0 > SystemParameters.WorkArea.Height)
            {
                minTop = (int)(SystemParameters.WorkArea.Height - deskLogicalHeight - bpHeight - 12.0);
                if (minTop < 6 )
                {
                    minTop = 6;
                }
            }
            if (minLeft == 0 || minTop == 0)
            {
                minLeft = minLeft ==0 ? Desk.Rect.Left : minLeft;
                minTop = minTop == 0 ? Desk.Rect.Top : minTop;
                Desk.SetLocation(minLeft, minTop);
            }
        }


        private void SetRightPanelPosition()
        {
            if (IsShowRightPanel)
            {
                SetPositionToDeskLeftTop(ctlRightPanel, Desk.Rect.Width + 6, 0);
                if (ctlRightPanel.Visibility > Visibility.Visible)
                {
                    ctlRightPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                SetPositionToDeskLeftTop(btnShowRight, Desk.Rect.Width - (int)btnShowRight.Width - 5, - (int)btnShowRight.Height - 5);
                if (btnShowRight.Visibility > Visibility.Visible)
                {
                    btnShowRight.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetPositionToDeskLeftTop(UIElement uie, int left, int top)
        {
            try
            {
                System.Windows.Point p = PointFromScreen(new System.Windows.Point((double)(left + Desk.Rect.Left), (double)(top + Desk.Rect.Top)));
                MoveUIElement(uie, p, MoveTypeEnum.LeftTop);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private void MoveUIElement(UIElement uie, System.Windows.Point p, MoveTypeEnum mtype)
        {
            if (!IsLocationEqual(uie, p, mtype))
            {
                if (uie.Visibility == Visibility.Visible)
                {
                    uie.Visibility = Visibility.Hidden;
                    MoveUIElementInner(uie, p, mtype);
                    uie.Visibility = Visibility.Visible;
                }
                else
                {
                    MoveUIElementInner(uie, p, mtype);
                }
            }
        }

        private void MoveUIElementInner(UIElement uie, System.Windows.Point p, MoveTypeEnum mtype)
        {
            if (mtype != MoveTypeEnum.LeftTop)
            {
                if (mtype != MoveTypeEnum.RightTop)
                {
                    Util.Assert(false);
                }
                else
                {
                    Canvas.SetRight(uie, p.X);
                    Canvas.SetTop(uie, p.Y);
                }
            }
            else
            {
                Canvas.SetLeft(uie, p.X);
                Canvas.SetTop(uie, p.Y);
            }
        }

        private bool IsLocationEqual(UIElement uie, System.Windows.Point p, MoveTypeEnum mtype)
        {
            bool rt;
            if (mtype != MoveTypeEnum.LeftTop)
            {
                if (mtype != MoveTypeEnum.RightTop)
                {
                    Util.Assert(false);
                    rt = false;
                }
                else
                {
                    rt = (Canvas.GetRight(uie) == p.X && Canvas.GetTop(uie) == p.Y);
                }
            }
            else
            {
                rt = (Canvas.GetLeft(uie) == p.X && Canvas.GetTop(uie) == p.Y);
            }
            return rt;
        }

        public static WndAssist GetTopWindow()
        {
            return WinApi.GetTopWindow<WndAssist>();
        }

        public static WndAssist FindWndAssist(string seller)
        {
            return WndAssist.AssistBag.FirstOrDefault(k=>k.Desk.WndTitle == seller);
        }

    }

}
