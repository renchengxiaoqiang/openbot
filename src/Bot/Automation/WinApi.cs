using BotLib;
using BotLib.Collection;
using BotLib.Extensions;
//using PeNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Bot.Automation
{
    public static class WinApi
    {
        public enum ClsNameEnum
        {
            Unknown,
            StandardWindow,
            StandardButton,
            SplitterBar,
            RichEditComponent,
            PrivateWebCtrl,
            CefBrowserWindow,
            Aef_WidgetWin_0,
            Chrome_WidgetWin_0,
            Chrome_RenderWidgetHostHWND,
            ToolBarPlus,
            Aef_RenderWidgetHostHWND,
            SuperTabCtrl,
            StackPanel,
            EditComponent,
            Qt5152QWindowIcon
        }
        public class WindowClue
        {
            public string ClsName
            {
                get;
                private set;
            }
            public string Text
            {
                get;
                private set;
            }
            public int SkipCount
            {
                get;
                private set;
            }
            public WindowClue(ClsNameEnum clsname, string text = null, int skipCount = -1)
            {
                this.ClsName = clsname.ToString();
                if (text == "")
                {
                    text = null;
                }
                this.Text = text;
                this.SkipCount = skipCount;
            }
            public WindowClue(string clsname, string text = null, int skipCount = -1)
            {
                this.ClsName = clsname;
                this.Text = ((text == "") ? null : text);
                this.SkipCount = skipCount;
            }
        }
        public class WindowPlacement
        {
            public Rectangle RestoreWindow;
            public WindowState WinState;
            public override string ToString()
            {
                return string.Format("rect:left={0},top={1},width={2},height={3}.state={4}", new object[]
				{
					RestoreWindow.Left,
					RestoreWindow.Top,
					RestoreWindow.Width,
					RestoreWindow.Height,
					WinState
				});
            }
        }
        public static class Editor
        {
            private static object _pasteRichTextSynObj;
            public static bool GetText(HwndInfo hwnd, out string txt)
            {
                bool rlt = HwndTextReaderFromTalkerNoAi.GetText(hwnd.Handle, out txt);
                txt = (txt ?? "");
                return rlt;
            }
            public static bool GetText(HwndInfo hwnd, out string txt, out string err, bool isvip = false)
            {
                bool text = HwndTextReaderFromTalkerNoAi.GetText(hwnd.Handle, out txt, out err, isvip);
                txt = (txt ?? "");
                return text;
            }
            public static bool SetText(HwndInfo hwnd, string txt, bool moveCaretToEnd = false)
            {
                var sb = new StringBuilder();
                sb.Append(txt);
                bool result = HwndTextReaderFromTalkerNoAi.MessageSender.SendForSetText(hwnd, sb, 2000);
                if (moveCaretToEnd)
                {
                    MoveCaretToEnding(hwnd);
                }
                return result;
            }
            public static bool SelectTextAll(HwndInfo hwnd)
            {
                return HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "SelectTextAll", 177, 0, -1, 2000);
            }
            public static bool MoveCaretToEnding(HwndInfo hwnd)
            {
                return HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "MoveCaretToEnding", 177, -1, -1, 2000);
            }
            public static void PasteRichText(HwndInfo hwnd, string rtf)
            {
                lock (_pasteRichTextSynObj)
                {
                    Dictionary<string, object> dict = ClipboardEx.Backup();
                    ClipboardEx.SetRichText(rtf);
                    PasteText(hwnd);
                    ClipboardEx.Restore(dict);
                }
            }
            public static void Test()
            {
            }
            static Editor()
            {
                _pasteRichTextSynObj = new object();
            }
        }
        private class HwndTextReaderFromTalkerNoAi
        {
            private static Cache<int, bool> _hwndAccessedCache;
            private static List<int> _hwndAccessedList;
            private static Dictionary<int, int> _badHwndDict;
            public static bool GetText(int hWnd, out string txt)
            {
                bool result = false;
                txt = null;
                if (hWnd == 0)
                {
                    return result;
                }
                if (IsBadHwnd(hWnd))
                {
                    txt = "";
                    return result;
                }
                if (!HasHwndOrAdd(hWnd))
                {
                    result = GetHwndText(hWnd, out txt);
                }
                else
                {
                    var sb = new StringBuilder(2048);
                    result = MessageSender.SendForGetText(hWnd, sb, 2000);
                    txt = sb.ToString();
                }
                return result;
            }
            public static bool GetHwndText(int hWnd, out string txt)
            {
                txt = null;
                bool result = false;
                if (hWnd == 0)
                {
                    return result;
                }
                if (HwndTextReaderFromTalkerNoAi.IsBadHwnd(hWnd))
                {
                    txt = "";
                    return result;
                }
                var sb = new StringBuilder(2048);
                if (result = MessageSender.SendForGetText(hWnd, sb, 2000))
                {
                    txt = sb.ToString();
                }
                return result;
            }
            private static bool HasHwndOrAdd(int hWnd)
            {
                bool rt = false;
                if (_hwndAccessedList.Contains(hWnd))
                {
                    rt = true;
                }
                else
                {
                    _hwndAccessedList.Add(hWnd);
                }
                return rt;
            }
            private static bool IsBadHwnd(int hWnd)
            {
                return _badHwndDict.ContainsKey(hWnd) && _badHwndDict[hWnd] > 5;
            }
            static HwndTextReaderFromTalkerNoAi()
            {
                _hwndAccessedCache = new Cache<int, bool>(1000, 60000, null);
                _hwndAccessedList = new List<int>();
                _badHwndDict = new Dictionary<int, int>();
            }
            public static bool GetText(int hwnd, out string txt, out string err, bool isvip)
            {
                bool rt = false;
                txt = null;
                err = null;
                if (hwnd == 0)
                {
                    txt = null;
                    err = "hwnd==0";
                }
                else if (!AddOrUpdateHwndAccessed(hwnd))
                {
                    if (!(rt = GetTextNoBlock(hwnd, out txt)))
                    {
                        err = "GetTextNoBlock";
                    }
                }
                else
                {
                    var stringBuilder = new StringBuilder(2048);
                    if (!(rt = MessageSender.SendForGetText(hwnd, stringBuilder, 2000, isvip)))
                    {
                        err = "SendForGetText";
                    }
                    txt = stringBuilder.ToString();
                }
                return rt;
            }

            public static bool GetTextNoBlock(int hwnd, out string txt)
            {
                txt = null;
                bool result = false;
                if (hwnd == 0)
                {
                    txt = null;
                }
                else
                {
                    var stringBuilder = new StringBuilder(2048);
                    if (result = MessageSender.SendForGetText(hwnd, stringBuilder, 2000, false))
                    {
                        txt = stringBuilder.ToString();
                    }
                }
                return result;
            }

            private static bool AddOrUpdateHwndAccessed(int hwnd)
            {
                bool rt = false;
                if (_hwndAccessedCache.ContainsKey(hwnd))
                {
                    rt = true;
                }
                else
                {
                    _hwndAccessedCache.AddOrUpdate(hwnd, true);
                }
                return rt;
            }
            public static class MessageSender
            {
                private class SendMsgSafe
                {
                    private int _hwnd;
                    private int _msg;
                    private int _wp;
                    private StringBuilder _sblp;
                    private int _ilp;
                    private ManualResetEvent _mre;
                    public bool IsFinished;
                    public SendMsgSafe(int hWnd, int msg, int wp, StringBuilder sblp, int msTimeout = 2000)
                    {
                        _mre = new ManualResetEvent(false);
                        IsFinished = false;
                        _hwnd = hWnd;
                        _msg = msg;
                        _wp = wp;
                        _sblp = sblp;
                        new Thread(new ThreadStart(this.SendForGetText))
                        {
                            IsBackground = true
                        }.Start();
                        if (_mre.WaitOne(msTimeout))
                        {
                            IsFinished = true;
                        }
                    }
                    public SendMsgSafe(int hWnd, int msg, int wp, int ilp, int msTimeout = 2000)
                    {
                        _mre = new ManualResetEvent(false);
                        IsFinished = false;
                        _hwnd = hWnd;
                        _msg = msg;
                        _wp = wp;
                        _ilp = ilp;
                        new Thread(new ThreadStart(this.SendForSetText))
                        {
                            IsBackground = true
                        }.Start();
                        if (this._mre.WaitOne(msTimeout))
                        {
                            this.IsFinished = true;
                        }
                    }
                    private void SendForGetText()
                    {
                        SendMessage(this._hwnd, this._msg, this._wp, this._sblp);
                        _mre.Set();
                    }
                    private void SendForSetText()
                    {
                        SendMessage(this._hwnd, this._msg, this._wp, this._ilp);
                        _mre.Set();
                    }
                    [DllImport("user32.dll")]
                    public static extern int SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);
                    [DllImport("user32.dll", EntryPoint = "SendMessage")]
                    public static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);
                }
                private class HwndSendMessageStatInfo
                {
                    static HwndSendMessageStatInfo()
                    {
                        _dict = new Cache<int, HwndSendMessageStatInfo>(1000, 0, null);
                    }
                    public HwndSendMessageStatInfo(int hwnd, string desc)
                    {
                        this._isInvalid = false;
                        this._hwnd = hwnd;
                        this._desc = desc;
                        this._lastOkTime = DateTime.MaxValue;
                        this._lastFailTime = DateTime.MaxValue;
                        this._lastStartFailTime = DateTime.MaxValue;
                    }
                    private static Cache<int, HwndSendMessageStatInfo> _dict;
                    private int _hwnd;
                    private string _desc;
                    private ulong _okCount;
                    private ulong _totalFailCount;
                    private ulong _lastFailCount;
                    private DateTime _lastOkTime;
                    private DateTime _lastFailTime;
                    private DateTime _lastStartFailTime;
                    private bool _isInvalid;
                    public static bool HwndCanAccess(int hWnd)
                    {
                        bool canAccess = true;
                        if (hWnd == 0)
                        {
                            canAccess = false;
                            return canAccess;
                        }
                        HwndSendMessageStatInfo hwndSendMessageStatInfo;
                        if (_dict.TryGetValue(hWnd, out hwndSendMessageStatInfo, null))
                        {
                            canAccess = hwndSendMessageStatInfo.CanAccess;
                        }
                        return canAccess;
                    }
                    public static void Report(HwndInfo hwndInfo, string desc, bool isOk)
                    {
                        try
                        {
                            if (!_dict.ContainsKey(hwndInfo.Handle))
                            {
                                _dict[hwndInfo.Handle] = new HwndSendMessageStatInfo(hwndInfo.Handle, desc + "," + hwndInfo.Description);
                            }
                            if (isOk)
                            {
                                _dict[hwndInfo.Handle].UpdateOkCount();
                            }
                            else
                            {
                                bool isInvalid;
                                string lastError = GetLastError(out isInvalid);
                                Log.Error(string.Format("MessageSender.HwndSendMessageStatInfo,LastError={0},adesc={1},hwndDesc={2},hwnd={3}", new object[]
							    {
								    lastError,
								    desc,
								    hwndInfo.Description,
								    hwndInfo.Handle
							    }));
                                _dict[hwndInfo.Handle].UpdateFailCount(isInvalid);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                    public static void Report(int hWnd, string desc, bool isOk)
                    {
                        try
                        {
                            if (!_dict.ContainsKey(hWnd))
                            {
                                _dict[hWnd] = new HwndSendMessageStatInfo(hWnd, desc);
                            }
                            if (isOk)
                            {
                                _dict[hWnd].UpdateOkCount();
                            }
                            else
                            {
                                bool isInvalid;
                                string lastError = GetLastError(out isInvalid);
                                _dict[hWnd].UpdateFailCount(isInvalid);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                    private bool CanAccess
                    {
                        get
                        {
                            return (!this._isInvalid && this._lastFailCount < 2) || (DateTime.Now - this._lastFailTime).TotalMinutes > 2.0;
                        }
                    }
                    public void UpdateOkCount()
                    {
                        this._okCount += 1;
                        this._lastOkTime = DateTime.Now;
                        this._lastStartFailTime = DateTime.MaxValue;
                        this._lastFailCount = 0;
                        this._isInvalid = false;
                    }
                    public void UpdateFailCount(bool isInvalid)
                    {
                        this._totalFailCount += 1uL;
                        this._lastFailCount += 1uL;
                        this._lastFailTime = DateTime.Now;
                        this._isInvalid = isInvalid;
                        if (this._lastStartFailTime == DateTime.MaxValue)
                        {
                            this._lastStartFailTime = this._lastFailTime;
                        }
                        this.LogFailIfNeed();
                    }

                    public static bool IsHwndOk(int hwnd)
                    {
                        bool rt = true;
                        try
                        {
                            if (_dict == null)
                            {
                                Log.Info("IsHwndOk, dict==null");
                            }
                            HwndSendMessageStatInfo hwndSendMessageStatInfo;
                            if (_dict != null && _dict.TryGetValue(hwnd, out hwndSendMessageStatInfo, null))
                            {
                                if (hwndSendMessageStatInfo == null)
                                {
                                    Log.Info("IsHwndOk,x==null");
                                }
                                if (hwndSendMessageStatInfo != null && (hwndSendMessageStatInfo._lastFailCount > 20uL || (hwndSendMessageStatInfo._lastFailTime != DateTime.MaxValue && (DateTime.Now - hwndSendMessageStatInfo._lastFailTime).TotalSeconds < 5.0 && hwndSendMessageStatInfo._lastFailCount > 2uL)))
                                {
                                    rt = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                        return rt;
                    }
                    private void LogFailIfNeed()
                    {
                        if (this._totalFailCount == 10uL || this._totalFailCount % 1000uL == 0uL)
                        {
                            string msg = string.Format("send message fail.hwnd={0},desc={1},total fail count={2},\r\n                            last continue fail count={3},last start fail time={4},last fail time={5},\r\n                            ok count={6},last ok time={7}", new object[]
						{
							this._hwnd,
							this._desc,
							this._totalFailCount,
							this._lastFailCount,
							this._lastStartFailTime,
							this._lastFailTime,
							this._okCount,
							this._lastOkTime
						});
                            Log.Error(msg);
                        }
                    }
                }

                public static bool SendForGetText(int hwnd, StringBuilder sb, int timeoutMs = 2000, bool isvip = false)
                {
                    bool rt = false;
                    if (hwnd != 0 && (isvip || HwndSendMessageStatInfo.IsHwndOk(hwnd)))
                    {
                        int lpdwResult;
                        rt = Api.SendMessageTimeout(hwnd, 13, sb.Capacity, sb, Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, timeoutMs, out lpdwResult);
                        HwndSendMessageStatInfo.Report(hwnd, "SendForGetText", rt);
                    }
                    return rt;
                }
                public static bool SendForGetText(int hWnd, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (HwndSendMessageStatInfo.HwndCanAccess(hWnd))
                    {
                        int lpdwResult;
                        result = Api.SendMessageTimeout(hWnd, 13, sbText.Capacity, sbText, Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        HwndSendMessageStatInfo.Report(hWnd, "SendForGetText", result);
                    }
                    return result;
                }
                public static bool SendForSetText(int hWnd, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (HwndSendMessageStatInfo.HwndCanAccess(hWnd))
                    {
                        int lpdwResult;
                        result = Api.SendMessageTimeout(hWnd, 12, 0, sbText, Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        HwndSendMessageStatInfo.Report(hWnd, "SendForSetText", result);
                    }
                    return result;
                }
                public static bool SendForGetText(HwndInfo hwndInfo, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (HwndSendMessageStatInfo.HwndCanAccess(hwndInfo.Handle))
                    {
                        int lpdwResult;
                        result = Api.SendMessageTimeout(hwndInfo.Handle, 13, sbText.Capacity, sbText, Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        HwndSendMessageStatInfo.Report(hwndInfo, "SendForGetText", result);
                    }
                    return result;
                }
                public static bool SendForSetText(HwndInfo hwndInfo, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (HwndSendMessageStatInfo.HwndCanAccess(hwndInfo.Handle))
                    {
                        int lpdwResult;
                        result = Api.SendMessageTimeout(hwndInfo.Handle, 12, 0, sbText, Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        HwndSendMessageStatInfo.Report(hwndInfo, "SendForSetText", result);
                    }
                    return result;
                }
                public static bool SendMessage(int hWnd, int msg, int wParam, int lParam, int uTimeout = 2000, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
                {
                    HwndInfo hwndInfo = new HwndInfo(hWnd, "unknown");
                    int lpdwResult;
                    return SendMessage(hwndInfo, caller, msg, wParam, lParam, out lpdwResult, uTimeout);
                }
                public static bool SendMessage(HwndInfo hwndInfo, string caller, int msg, int wParam, int lParam, out int lpdwResult, int uTimeout = 2000)
                {
                    bool result = false;
                    lpdwResult = 0;
                    if (HwndSendMessageStatInfo.HwndCanAccess(hwndInfo.Handle))
                    {
                        result = Api.SendMessageTimeout(hwndInfo.Handle, msg, wParam, lParam, Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        HwndSendMessageStatInfo.Report(hwndInfo, caller, result);
                    }
                    return result;
                }
                public static bool SendMessage(HwndInfo hWnd, string caller, int msg, int wParam, int lParam, int uTimeout = 2000)
                {
                    bool result = false;
                    if (HwndSendMessageStatInfo.HwndCanAccess(hWnd.Handle))
                    {
                        int lpdwResult;
                        result = Api.SendMessageTimeout(hWnd.Handle, msg, wParam, lParam, Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        HwndSendMessageStatInfo.Report(hWnd, caller, result);
                    }
                    return result;
                }
            }
        }

        public static class ChildHwndManager
        {
            private class ChildHwndCollection
            {
                private int ParentHwnd;
                private List<int> ChildHwnds = new List<int>();
                private DateTime objNewTime;
                public ChildHwndCollection(int parentHwnd, List<int> childHwnds)
                {
                    this.ParentHwnd = parentHwnd;
                    this.ChildHwnds = childHwnds;
                    this.objNewTime = DateTime.Now;
                }
                public bool IsGreaterThenTwoSecd()
                {
                    return (DateTime.Now - this.objNewTime).TotalSeconds > (double)TwoSecd;
                }
                public bool ContainsHwnd(int childHwnd)
                {
                    return this.ChildHwnds.Contains(childHwnd);
                }
            }
            private static readonly int TwoSecd = 2;
            private static Dictionary<int, ChildHwndCollection> _childHwndCache = new Dictionary<int, ChildHwndCollection>(10, null);
            public static bool IsChild(int parentHwnd, int childHwnd)
            {
                if (!_childHwndCache.ContainsKey(parentHwnd) || _childHwndCache[parentHwnd].IsGreaterThenTwoSecd())
                {
                    List<int> childHwnds = GetChildHwnds(parentHwnd);
                    ChildHwndCollection value = new ChildHwndCollection(parentHwnd, childHwnds);
                    _childHwndCache[parentHwnd] = value;
                }
                return _childHwndCache[parentHwnd].ContainsHwnd(childHwnd);
            }
            private static List<int> GetChildHwnds(int pHwnd)
            {
                List<int> childHwnds = new List<int>();
                int hwnd = 0;
                while (true)
                {
                    hwnd = Api.FindWindowEx(pHwnd, hwnd, null, null);
                    if (hwnd == 0)
                    {
                        break;
                    }
                    childHwnds.Add(hwnd);
                    childHwnds.AddRange(GetChildHwnds(hwnd));
                }
                return childHwnds;
            }
        }
        public static class Api
        {
            public enum ShowWindowCommands
            {
                Hide,
                Normal,
                ShowMinimized,
                Maximize,
                ShowMaximized = 3,
                ShowNoActivate,
                Show,
                Minimize,
                ShowMinNoActive,
                ShowNA,
                Restore,
                ShowDefault,
                ForceMinimize
            }
            private enum GetWindow_Cmd : uint
            {
                GW_HWNDFIRST,
                GW_HWNDLAST,
                GW_HWNDNEXT,
                GW_HWNDPREV,
                GW_OWNER,
                GW_CHILD,
                GW_ENABLEDPOPUP
            }
            public static class SWP
            {
                public static readonly int NOSIZE;
                public static readonly int NOMOVE;
                public static readonly int NOZORDER;
                public static readonly int NOREDRAW;
                public static readonly int NOACTIVATE;
                public static readonly int DRAWFRAME;
                public static readonly int FRAMECHANGED;
                public static readonly int SHOWWINDOW;
                public static readonly int HIDEWINDOW;
                public static readonly int NOCOPYBITS;
                public static readonly int NOOWNERZORDER;
                public static readonly int NOREPOSITION;
                public static readonly int NOSENDCHANGING;
                public static readonly int DEFERERASE;
                public static readonly int ASYNCWINDOWPOS;
                static SWP()
                {
                    NOSIZE = 1;
                    NOMOVE = 2;
                    NOZORDER = 4;
                    NOREDRAW = 8;
                    NOACTIVATE = 16;
                    DRAWFRAME = 32;
                    FRAMECHANGED = 32;
                    SHOWWINDOW = 64;
                    HIDEWINDOW = 128;
                    NOCOPYBITS = 256;
                    NOOWNERZORDER = 512;
                    NOREPOSITION = 512;
                    NOSENDCHANGING = 1024;
                    DEFERERASE = 8192;
                    ASYNCWINDOWPOS = 16384;
                }
            }
            public enum GetWindowType : uint
            {
                GW_HWNDFIRST,
                GW_HWNDLAST,
                GW_HWNDNEXT,
                GW_HWNDPREV,
                GW_OWNER,
                GW_CHILD,
                GW_ENABLEDPOPUP
            }
            public enum ShowCmdEnum
            {
                HIDE,
                SHOWNORMAL,
                NORMAL = 1,
                SHOWMINIMIZED,
                SHOWMAXIMIZED,
                MAXIMIZE = 3,
                SHOWNOACTIVATE,
                SHOW,
                MINIMIZE,
                SHOWMINNOACTIVE,
                SHOWNA,
                RESTORE,
                SHOWDEFAULT,
                FORCEMINIMIZE
            }
            public struct Point
            {
                public int x;
                public int y;
                public Point(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                }
            }
            public struct Rect
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            public struct WindowPlacement
            {
                public int length;
                public uint flags;
                public ShowCmdEnum showCmd;
                public Point ptMinPosition;
                public Point ptMaxPosition;
                public Rect rcNormalPosition;
            }
            [Flags]
            public enum SendMessageTimeoutFlags : uint
            {
                SMTO_NORMAL = 0u,
                SMTO_BLOCK = 1u,
                SMTO_ABORTIFHUNG = 2u,
                SMTO_NOTIMEOUTIFNOTHUNG = 8u,
                SMTO_ERRORONEXIT = 32u
            }
            public static readonly int HWND_TOPMOST;
            public static readonly int HWND_NOTOPMOST;
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool ShowWindow(int hWnd, Api.ShowWindowCommands cmd);
            [DllImport("user32.dll")]
            public static extern bool ShowWindowAsync(int hWnd, Api.ShowWindowCommands cmd);
            [DllImport("user32")]
            private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
            [DllImport("user32.dll")]
            public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
            [DllImport("user32.dll")]
            public static extern bool IsWindowEnabled(int hWnd);
            [DllImport("user32.dll")]
            public static extern int GetTopWindow(int hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(int hWnd, int nlndex);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(int hWnd, int nlndex, int dwNewLong);
            public static int GetNextWindow(int hWnd)
            {
                return Api.GetWindow(hWnd, Api.GetWindow_Cmd.GW_HWNDNEXT);
            }
            public static int GetPrevWindow(int hWnd)
            {
                return Api.GetWindow(hWnd, Api.GetWindow_Cmd.GW_HWNDPREV);
            }
            [DllImport("user32.dll", SetLastError = true)]
            private static extern int GetWindow(int hWnd, Api.GetWindow_Cmd cmd);
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
            [DllImport("user32.dll")]
            public static extern int IsIconic(int hWnd);
            [DllImport("user32.dll")]
            public static extern int IsZoomed(int hwnd);
            [DllImport("user32.dll")]
            public static extern int GetWindowRect(int hWnd, ref Api.Rect rect);
            [DllImport("user32.dll")]
            public static extern int WindowFromPoint(Api.Point pt);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int GetAncestor(int int_2, int int_3);
            [DllImport("user32.dll", EntryPoint = "GetWindow", SetLastError = true)]
            public static extern int GetWindow(int hWnd, Api.GetWindowType type);
            [DllImport("user32.dll")]
            public static extern int GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool GetGUIThreadInfo(int idThread, ref Api.GuiThreadInfo pgui);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetClassName(int hwnd, StringBuilder lpClassName, int nMaxCount);
            [DllImport("user32.dll")]
            public static extern bool GetWindowPlacement(int hWnd, ref Api.WindowPlacement lpwndpl);
            [DllImport("user32.dll")]
            public static extern bool SetWindowPlacement(int hWnd, ref Api.WindowPlacement lpwndpl);
            [DllImport("user32.dll")]
            public static extern int GetWindowThreadProcessId(int hwnd, ref int lpdwProcessId);
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindow(int hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool SendMessageTimeout(int hWnd, int Msg, int wParam, int lParam, Api.SendMessageTimeoutFlags fuFlags, int uTimeout, out int lpdwResult);
            [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessageTimeout", SetLastError = true)]
            public static extern bool SendMessageTimeout(int hWnd, int Msg, int wParam, StringBuilder lParam, Api.SendMessageTimeoutFlags fuFlags, int uTimeout, out int lpdwResult);
            [DllImport("user32.dll")]
            public static extern int SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);
            [DllImport("user32.dll", EntryPoint = "SendMessage")]
            public static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]
            public static extern int PostMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);
            [DllImport("user32.dll", EntryPoint = "PostMessage")]
            public static extern int PostMessage(int hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]
            public static extern int FindWindowEx(int hwndParent, int hwndChildAfter, string lpszClass, string lpszWindow);
            [DllImport("user32.dll")]
            public static extern bool IsWindowVisible(int hWnd);
            public struct GuiThreadInfo
            {
                public int cbSize;
                public uint flags;
                public int hwndActive;
                public int hwndFocus;
                public int hwndCapture;
                public int hwndMenuOwner;
                public int hwndMoveSize;
                public int hwndCaret;
                public Rect rcCaret;
            }

            static Api()
            {
                HWND_TOPMOST = -1;
                HWND_NOTOPMOST = -2;
            }
        }
        public static int GetZOrderHighestHwnd(HashSet<int> hlist)
        {
            for (int i = Api.GetTopWindow(0); i > 0; i = Api.GetNextWindow(i))
            {
                if (hlist.Contains(i))
                {
                    return i;
                }
            }
            throw new Exception("GetZOrderHighestHwnd,找不到顶层窗口,hlist=" + hlist.xToString(",", true));
        }
        public static string GetLastError(out bool isInvalid)
        {
            string text = null;
            isInvalid = false;
            try
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                int num = lastWin32Error;
                if (num != 1400)
                {
                    if (num == 1460)
                    {
                        text = "ERROR_TIMEOUT";
                    }
                }
                else
                {
                    text = "ERROR_INVALID_WINDOW_HANDLE";
                    isInvalid = true;
                }
                text = ((text == null) ? ("0x" + lastWin32Error.ToString("x")) : string.Format("{0}(0x{1})", text, lastWin32Error.ToString("x")));
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return text;
        }
        public static bool SetFocus(HwndInfo hwnd)
        {
            return HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "focus", 7, 0, 0, 2000);
        }
        public static bool IsWindowEnabled(int hwnd)
        {
            return hwnd > 0 && Api.IsWindowEnabled(hwnd);
        }
        public static void BringTopAndSetWindowSize(int hwnd, int x, int y, int cx, int cy)
        {
            Api.SetWindowPos(hwnd, Api.HWND_TOPMOST, x, y, cx, cy, 64u);
        }
        public static bool BringTop(int hwnd)
        {
            //return TopMost(hwnd) && CancelTopMost(hwnd);
            bool rt = false;
            try
            {
                if (IsWindowMinimized(hwnd))
                {
                    ShowNormal(hwnd);
                }
                rt = (TopMost(hwnd) && CancelTopMost(hwnd) && SetForegroundWindow(hwnd));
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }
        public static bool TopMost(int hwnd)
        {
            return Api.SetWindowPos(hwnd, Api.HWND_TOPMOST, 1, 1, 1, 1, 67u);
        }
        public static bool CancelTopMost(int hwnd)
        {
            return Api.SetWindowPos(hwnd, Api.HWND_NOTOPMOST, 1, 1, 1, 1, 3u);
        }
        public static void ShowAfter(int hwnd, int insertafter)
        {
            if (!Api.SetWindowPos(hwnd, insertafter, 1, 1, 1, 1, 67u))
            {
                Log.Info("ShowAfterFail.");
            }
        }
        public static int FindChildHwnd(int parentHwnd, WindowClue clue)
        {
            int hWnd = 0;
            if (clue.SkipCount > 0)
            {
                for (int i = 0; i <= clue.SkipCount; i++)
                {
                    hWnd = Api.FindWindowEx(parentHwnd, hWnd, clue.ClsName, clue.Text);
                    if (hWnd == 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                hWnd = Api.FindWindowEx(parentHwnd, hWnd, clue.ClsName, clue.Text);
            }
            return hWnd;
        }
        public static string GetWindowStructInfo(int hwnd)
        {
            string result;
            if (hwnd == 0)
            {
                result = "";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetWindowClass(hwnd));
                TraverseChildHwnd(hwnd, (hchild, level) =>
                {
                    string winClass = "";
                    string winText = "";
                    bool isVisible = false;
                    try
                    {
                        winClass = GetWindowClass(hchild);
                        winText = GetText(hchild);
                        isVisible = IsVisible(hchild);
                    }
                    catch (Exception ex)
                    {
                        winClass = "Error:" + ex.Message;
                    }
                    sb.AppendLine(string.Format("{0} {1},{2},{3}", (level > 0) ? new string('+', level) : "", winClass, isVisible, winText));
                }, true, 0, null, null);
                result = sb.ToString();
            }
            return result;
        }
        public static int FindDescendantHwnd(int parentHwnd, List<WindowClue> clueList, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
        {
            int hWnd = FindDescendantHwnd(parentHwnd, clueList, 0);
            if (hWnd == 0)
            {
                Log.Error("获取hwnd失败！CallerName=" + callerName);
            }
            return hWnd;
        }
        private static int FindDescendantHwnd(int parentHwnd, List<WindowClue> clueList, int startIdx)
        {
            if (clueList == null || clueList.Count == 0)
            {
                throw new Exception("没有WindowClue");
            }
            int hwndChild = 0;
            WindowClue windowClue = clueList[startIdx];
            int skipCnt = (windowClue.SkipCount > 0) ? windowClue.SkipCount : 0;
            for (int i = 0; i <= skipCnt; i++)
            {
                hwndChild = Api.FindWindowEx(parentHwnd, hwndChild, windowClue.ClsName, windowClue.Text);
                if (hwndChild == 0)
                {
                    return hwndChild;
                }
            }
            if (startIdx < clueList.Count - 1)
            {
                int dehWnd = FindDescendantHwnd(hwndChild, clueList, startIdx + 1);
                if (dehWnd == 0)
                {
                    while (dehWnd == 0)
                    {
                        hwndChild = Api.FindWindowEx(parentHwnd, hwndChild, windowClue.ClsName, windowClue.Text);
                        if (hwndChild == 0)
                        {
                            return hwndChild;
                        }
                        dehWnd = FindDescendantHwnd(hwndChild, clueList, startIdx + 1);
                    }
                    hwndChild = dehWnd;
                }
                else
                {
                    hwndChild = dehWnd;
                }
            }
            return hwndChild;
        }
        public static bool ShowNormal(int hwnd)
        {
            Api.WindowPlacement windowPlacement = default(Api.WindowPlacement);
            bool result = false;
            if (Api.GetWindowPlacement(hwnd, ref windowPlacement))
            {
                windowPlacement.showCmd = Api.ShowCmdEnum.SHOWNORMAL;
                result = Api.SetWindowPlacement(hwnd, ref windowPlacement);
            }
            return result;
        }
        public static bool SetLocation(int handle, int left, int top)
        {
            bool result = false;
            Api.WindowPlacement windowPlacement = default(Api.WindowPlacement);
            if (Api.GetWindowPlacement(handle, ref windowPlacement))
            {
                if (windowPlacement.rcNormalPosition.left != left || windowPlacement.rcNormalPosition.top != top)
                {
                    int num = left - windowPlacement.rcNormalPosition.left;
                    windowPlacement.rcNormalPosition.left = left;
                    windowPlacement.rcNormalPosition.right = windowPlacement.rcNormalPosition.right + num;
                    num = top - windowPlacement.rcNormalPosition.top;
                    windowPlacement.rcNormalPosition.top = top;
                    windowPlacement.rcNormalPosition.bottom = windowPlacement.rcNormalPosition.bottom + num;
                    result = Api.SetWindowPlacement(handle, ref windowPlacement);
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        public static bool SetRect(int handle, int left, int top, int w, int h)
        {
            bool result = false;
            Api.WindowPlacement windowPlacement = default(Api.WindowPlacement);
            if (Api.GetWindowPlacement(handle, ref windowPlacement))
            {
                int num = windowPlacement.rcNormalPosition.right - windowPlacement.rcNormalPosition.left;
                int num2 = windowPlacement.rcNormalPosition.bottom - windowPlacement.rcNormalPosition.top;
                if (windowPlacement.rcNormalPosition.left != left || windowPlacement.rcNormalPosition.top != top || num != w || num2 != h)
                {
                    windowPlacement.rcNormalPosition.left = left;
                    windowPlacement.rcNormalPosition.right = left + w;
                    windowPlacement.rcNormalPosition.top = top;
                    windowPlacement.rcNormalPosition.bottom = top + h;
                    result = Api.SetWindowPlacement(handle, ref windowPlacement);
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        public static void ClickHwndBySendMessage(int hwnd, int maxTry = 1)
        {
            if (hwnd > 0)
            {
                if (maxTry < 1)
                {
                    maxTry = 1;
                }
                int num = 0;
                while (num < maxTry && !HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 245, 0, 0, 2000, "ClickHwndBySendMessage"))
                {
                    num++;
                }
            }
        }

        public static void ClickHwndByPostMessage(int hwnd)
        {
            Api.PostMessage(hwnd, 245, 0, 0);
        }
        public static bool IsWindowPointVisable(System.Drawing.Point point, int hwnd)
        {
            return IsWindowPointVisable(new Api.Point(point.X, point.Y), hwnd);
        }
        public static bool IsWindowPointVisable(Api.Point point, int hwnd)
        {
            bool result = false;
            if (IsPointInsideScreen(point.x, point.y))
            {
                int qnHwnd = Api.WindowFromPoint(point);
                if (hwnd == qnHwnd || IsChild(hwnd, qnHwnd))
                {
                    result = true;
                }
            }
            return result;
        }
        public static bool IsChild(int parentHwnd, int childHwnd)
        {
            return ChildHwndManager.IsChild(parentHwnd, childHwnd);
        }
        public static bool IsPointInsideScreen(int x, int y)
        {
            return x >= 0 && x < SystemParameters.WorkArea.Width && y >= 0 && y < SystemParameters.WorkArea.Height;
        }
        private static bool ClickPointBySendMessage(int hwnd, int x, int y)
        {
            var rt = false;
            if (x >= 0 && y >= 0)
            {
                y <<= 16;
                y = (x | y);
                HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 513, 1, y, 2000, "ClickPointBySendMessage");
                rt = HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 514, 0, y, 2000, "ClickPointBySendMessage");
            }
            return rt;
        }
        public static void ClickPointBySendMessage(int hwnd, int x, int y, int maxTry = 1)
        {
            if (hwnd > 0)
            {
                if (maxTry < 1)
                {
                    maxTry = 1;
                }
                int num = 0;
                while (num < maxTry && !ClickPointBySendMessage(hwnd,x,y))
                {
                    num++;
                }
            }
        }

        public static bool HideWindow(int hwnd)
        {
            bool result = true;
            try
            {
                result = Api.SetWindowPos(hwnd, 1, 1, 1, 50, 50,131u);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                result = false;
            }
            return result;
        }

        public static bool CancelHideWindow(int hwnd)
        {
            bool result = true;
            try
            {
                if (!Api.IsWindowVisible(hwnd))
                {
                    Api.SetWindowPos(hwnd, 1, 1, 1, 1, 1,67u);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                result = false;
            }
            return result;
        }
        public static bool IsWindowMinimized(int hwnd)
        {
            return Api.IsIconic(hwnd) > 0;
        }
        public static bool IsWindowMaximized(int hwnd)
        {
            return Api.IsZoomed(hwnd) > 0;
        }
        public static string GetWindowClass(int hwnd)
        {
            var sb = new StringBuilder(512);
            Api.GetClassName(hwnd, sb, 512);
            return sb.ToString();
        }
        public static bool IsHwndAlive(int hwnd)
        {
            return Api.IsWindow(hwnd);
        }
        public static int GetWindowThreadProcessId(int hWnd)
        {
            int pid = 0;
            Api.GetWindowThreadProcessId(hWnd, ref pid);
            return pid;
        }
        public static int GetWindowThreadProcessId(int hWnd, out int pid)
        {
            pid = 0;
            return Api.GetWindowThreadProcessId(hWnd, ref pid);
        }
        public static bool IsVisible(int hwnd)
        {
            return Api.IsWindowVisible(hwnd);
        }
        public static Task<bool> CloseWindowAsyn(int hwnd, int timeoutMs = 3000)
        {
            return Task.Factory.StartNew<bool>(() => CloseWindow(hwnd, timeoutMs));
        }
        public static bool CloseWindow(int hwnd, int timeoutMs = 2000)
        {
            return HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 16, 0, 0, timeoutMs, "CloseWindow");
        }
        public static bool IsForeground(int hwnd)
        {
            return Api.GetForegroundWindow() == hwnd && !IsWindowMinimized(hwnd);
        }
        public static bool BringTopAndDoAction(int hwnd, Action act, int cnt = 2)
        {
            bool rt = false;
            int i = 0;
            while (i < cnt)
            {
                if (!IsForeground(hwnd))
                {
                    BringTop(hwnd);
                    if (!IsForeground(hwnd))
                    {
                        Thread.Sleep(50);
                        i++;
                        continue;
                    }
                    if (act != null)
                    {
                        act();
                    }
                    rt = true;
                }
                else
                {
                    if (act != null)
                    {
                        act();
                    }
                    rt = true;
                }
                return rt;
            }
            return rt;
        }
        public static WindowPlacement GetWindowPlacement(int hwnd)
        {
            WindowPlacement result;
            try
            {
                AssertHwndNonZero(hwnd);
                Api.WindowPlacement windowPlacement = default(Api.WindowPlacement);
                windowPlacement.length = Marshal.SizeOf(windowPlacement);
                if (!Api.GetWindowPlacement(hwnd, ref windowPlacement))
                {
                    throw new Exception("GetWindowPlacement return false");
                }
                WindowPlacement windowPlacement2 = new WindowPlacement();
                windowPlacement2.RestoreWindow = new Rectangle(windowPlacement.rcNormalPosition.left, windowPlacement.rcNormalPosition.top, windowPlacement.rcNormalPosition.right - windowPlacement.rcNormalPosition.left, windowPlacement.rcNormalPosition.bottom - windowPlacement.rcNormalPosition.top);
                switch (windowPlacement.showCmd)
                {
                    case (Api.ShowCmdEnum)1:
                        windowPlacement2.WinState = WindowState.Normal;
                        break;
                    case (Api.ShowCmdEnum)2:
                        windowPlacement2.WinState = WindowState.Minimized;
                        break;
                    case (Api.ShowCmdEnum)3:
                        windowPlacement2.WinState = WindowState.Maximized;
                        break;
                    default:
                        throw new Exception("GetWindowPlacement,unknown showCmd=" + windowPlacement.showCmd);
                }
                result = windowPlacement2;
                return result;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            result = null;
            return result;
        }
        public static bool RestoreWindow(int hwnd)
        {
            Api.WindowPlacement windowPlacement = default(Api.WindowPlacement);
            bool result = false;
            if (Api.GetWindowPlacement(hwnd, ref windowPlacement))
            {
                windowPlacement.showCmd = Api.ShowCmdEnum.RESTORE;
                result = Api.SetWindowPlacement(hwnd, ref windowPlacement);
            }
            return result;
        }
        public static bool SetWindowState(int hwnd, WindowState states)
        {
            Api.WindowPlacement windowPlacement = default(Api.WindowPlacement);
            bool rt = false;
            if (Api.GetWindowPlacement(hwnd, ref windowPlacement))
            {
                bool flag = false;
                switch (states)
                {
                    case WindowState.Normal:
                        if (windowPlacement.showCmd != Api.ShowCmdEnum.SHOWNORMAL)
                        {
                            windowPlacement.showCmd = Api.ShowCmdEnum.SHOWNORMAL;
                            flag = true;
                        }
                        break;
                    case WindowState.Minimized:
                        if (windowPlacement.showCmd != Api.ShowCmdEnum.MINIMIZE)
                        {
                            windowPlacement.showCmd = Api.ShowCmdEnum.MINIMIZE;
                            flag = true;
                        }
                        break;
                    case WindowState.Maximized:
                        if (windowPlacement.showCmd != Api.ShowCmdEnum.SHOWMAXIMIZED)
                        {
                            windowPlacement.showCmd = Api.ShowCmdEnum.SHOWMAXIMIZED;
                            flag = true;
                        }
                        break;
                }
                rt = (!flag || Api.SetWindowPlacement(hwnd, ref windowPlacement));
            }
            return rt;
        }
        private static void AssertHwndNonZero(int hwnd)
        {
            Util.Assert(hwnd > 0);
        }
        public static void TraverseAllHwnd(Action<int> action, string className = null, string title = null)
        {
            TraverseChildHwnd(0, action, true, className, title);
        }
        public static void TraverAllDesktopHwnd(Action<int> action, string className = null, string title = null, int pid = 0)
        {
            TraverseChildHwnd(0, action, false, className, title, pid);
        }
        public static void TraverDesktopHwnd(Func<int, bool> func, string className = null, string title = null)
        {
            TraverseChildHwnd(0, func, false, className, title);
        }
        public static void TraverseChildHwnd(int parent, Func<int, bool> func, bool traverseDescandent, string className = null, string title = null, int pid = 0)
        {
            for (int i = Api.FindWindowEx(parent, 0, className, title); i > 0; i = Api.FindWindowEx(parent, i, className, title))
            {
                var rt = true;
                if (i == 0 || pid == 0 || GetWindowThreadProcessId(i) == pid)
                {
                    rt = func(i);
                }
                if (!rt)
                {
                    break;
                }
                if (traverseDescandent)
                {
                    TraverseChildHwnd(i, func, traverseDescandent, className, title);
                }
            }
        }
        public static void TraverseChildHwnd(int parent, Action<int> act, bool traverseDescandent, string className = null, string title = null, int pid = 0)
        {
            for (int i = Api.FindWindowEx(parent, 0, className, title); i > 0; i = Api.FindWindowEx(parent, i, className, title))
            {
                if (i == 0 || pid == 0 || GetWindowThreadProcessId(i) == pid)
                {
                    act(i);
                    if (traverseDescandent)
                    {
                        TraverseChildHwnd(i, act, traverseDescandent, className, title);
                    }
                }
            }
        }
        public static void TraverseChildHwnd(int parent, Action<int, int> act, bool traverseDescandent, int level, string className = null, string title = null)
        {
            int i = Api.FindWindowEx(parent, 0, className, title);
            level++;
            while (i > 0)
            {
                act(i, level);
                if (traverseDescandent)
                {
                    TraverseChildHwnd(i, act, traverseDescandent, level, className, title);
                }
                i = Api.FindWindowEx(parent, i, className, title);
            }
        }
        public static List<int> FindAllWindowByTitlePattern(string pattern)
        {
            List<int> rtlist = new List<int>();
            TraverseAllHwnd(hWnd =>
            {
                string input;
                if (GetText(new HwndInfo(hWnd, "FindAllWindowByTitlePattern"), out input) && Regex.IsMatch(input, pattern))
                {
                    rtlist.Add(hWnd);
                }
            }, null, null);
            return rtlist;
        }
        public static List<int> FindAllDesktopWindowWithClassName(string cname)
        {
            List<int> rtlist = new List<int>();
            TraverAllDesktopHwnd((int hwnd) =>
            {
                rtlist.Add(hwnd);
            }, cname, null);
            return rtlist;
        }
        public static int FindFirstDesktopWindowByClassNameAndTitle(string cname, string title)
        {
            return Api.FindWindowEx(0, 0, cname, title);
        }
        public static List<int> FindAllDesktopWindowByClassNameAndTitlePattern(string cname, string pattern = null, Action<int, string> action = null, int processId = 0)
        {
            List<int> rtlist = new List<int>();
            TraverAllDesktopHwnd((int hwnd) =>
            {
                bool isMatch = true;
                string title = null;
                if (!string.IsNullOrEmpty(pattern))
                {
                    isMatch = (GetText(new HwndInfo(hwnd, "FindAllDesktopWindowByClassNameAndTitlePattern"), out title) && Regex.IsMatch(title, pattern));
                    var abc = title;
                }
                if (isMatch)
                {
                    rtlist.Add(hwnd);
                    if (action != null)
                    {
                        action(hwnd, title);
                    }
                }
            }, cname, null, processId);
            return rtlist;
        }
        public static bool GetText(HwndInfo hwnd, out string title)
        {
            return Editor.GetText(hwnd, out title);
        }
        public static string GetText(int hwnd)
        {
            string text;
            if (!Editor.GetText(new HwndInfo(hwnd, "GetText"), out text))
            {
                text = null;
            }
            return text;
        }
        public static void PressDot()
        {
            Api.keybd_event(110, 0, 0, 0);
            Api.keybd_event(110, 0, 2, 0);
        }
        public static void PressBackSpace()
        {
            Api.keybd_event(8, 0, 0, 0);
            Api.keybd_event(8, 0, 2, 0);
        }
        public static void PressEnterKey()
        {
            Api.keybd_event(13, 0, 0, 0);
            Api.keybd_event(13, 0, 2, 0);
        }
        public static void PressF12()
        {
            Api.keybd_event(123, 0, 0, 0);
            Api.keybd_event(123, 0, 2, 0);
        }
        public static void PressCtrlV()
        {
            Api.keybd_event(17, 0, 0, 0);
            Api.keybd_event(86, 0, 0, 0);
            Api.keybd_event(86, 0, 2, 0);
            Api.keybd_event(17, 0, 2, 0);
        }
        public static void PressCtrlA()
        {
            Api.keybd_event(17, 0, 0, 0);
            Api.keybd_event(65, 0, 0, 0);
            Api.keybd_event(65, 0, 2, 0);
            Api.keybd_event(17, 0, 2, 0);
        }

        public static void PressEnter()
        {
            Api.keybd_event(17, 0, 0, 0);
            Api.keybd_event(13, 0, 0, 0);
            Api.keybd_event(13, 0, 2, 0);
            Api.keybd_event(17, 0, 2, 0);
        }
        public static void PressEsc()
        {
            Api.keybd_event(17, 0, 0, 0);
            Api.keybd_event(192, 0, 0, 0);
            Api.keybd_event(192, 0, 2, 0);
            Api.keybd_event(17, 0, 2, 0);
        }
        public static void FocusWnd(int hWnd)
        {
            if (hWnd != 0)
            {
                Api.PostMessage(hWnd, 256, 40, 0);
                Api.PostMessage(hWnd, 257, 40, 0);
            }
        }
        public static bool FocusWnd(HwndInfo hwnd)
        {
            return HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "focus", 7, 0, 0, 2000);
        }
        public static bool PasteText(HwndInfo hwnd)
        {
            return HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "Paste", 770, 0, 0, 2000);
        }
        public static int GetOwner(int hwnd)
        {
            return Api.GetWindow(hwnd, Api.GetWindowType.GW_OWNER);
        }
        public static int GetRootHwnd(int hwnd)
        {
            return Api.GetAncestor(hwnd, 2);
        }
        public static int WindowFromPoint(System.Drawing.Point p)
        {
            return Api.WindowFromPoint(new Api.Point(p.X, p.Y));
        }
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(int hwnd);
        public static Rectangle GetWindowRectangle(int hwnd)
        {
            Rectangle rectRlt;
            if (hwnd > 0)
            {
                Api.Rect rect = default(Api.Rect);
                Api.GetWindowRect(hwnd, ref rect);
                rectRlt = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            }
            else
            {
                rectRlt = default(Rectangle);
            }
            return rectRlt;
        }


        private static double _sumSeconds;
        private static int _sumTimes;
        public static Bitmap GetWindowCapture(IntPtr handle, bool cancelMinimize)
        {
            Bitmap result = null;
            try
            {
                User32.RECT lpRect = default(User32.RECT);
                User32.GetWindowRect(handle, ref lpRect);
                var rect = new Rectangle(0, 0, lpRect.right + 1, lpRect.bottom + 1);
                GetWindowCapture(handle, rect, cancelMinimize);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return result;
        }

        public static Bitmap GetWindowCapture(IntPtr srcHwnd, Rectangle rect, bool cancelMinimize)
        {
            Bitmap bitmap = null;
            try
            {
                if (cancelMinimize || !IsWindowMinimized(srcHwnd.ToInt32()))
                {
                    var now = DateTime.Now;
                    var dc = User32.GetWindowDC(srcHwnd);
                    IntPtr hdc = GDI32.CreateCompatibleDC(dc);
                    try
                    {
                        if (cancelMinimize)
                        {
                            User32.ShowWindow(srcHwnd, 1);
                        }
                        User32.RECT lpRect = default(User32.RECT);
                        User32.GetWindowRect(srcHwnd, ref lpRect);
                        IntPtr hgdiobj = IntPtr.Zero;
                        IntPtr newhgdiobj = IntPtr.Zero;
                        try
                        {
                            hgdiobj = GDI32.CreateCompatibleBitmap(dc, rect.Width, rect.Height);
                            newhgdiobj = GDI32.SelectObject(hdc, hgdiobj);
                            GDI32.BitBlt(hdc, 0, 0, rect.Width, rect.Height, dc, rect.Left, rect.Top, 1087111200);
                            bitmap = Image.FromHbitmap(hgdiobj, IntPtr.Zero);
                            bitmap = (bitmap.Clone() as Bitmap);
                        }
                        finally
                        {
                            if (newhgdiobj != IntPtr.Zero)
                            {
                                GDI32.SelectObject(hdc, newhgdiobj);
                            }
                            if (hgdiobj != IntPtr.Zero)
                            {
                                GDI32.DeleteObject(hgdiobj);
                            }
                        }
                    }
                    finally
                    {
                        User32.ReleaseDC(srcHwnd, dc);
                        GDI32.DeleteDC(hdc);
                    }
                    _sumSeconds += (DateTime.Now - now).TotalMilliseconds;
                    _sumTimes++;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return bitmap;
        }

        public static void SetTransparentWindow(int hwnd)
        {
            Api.SetWindowLong(hwnd, -20, (Api.GetWindowLong(hwnd, -20) & -262145) | 128);
            Api.SetWindowPos(hwnd, 0, 0, 0, 0, 0, 35u);
        }
        public static int GetFocusHwnd()
        {
            int hWnd = 0;
            var guiThreadInfo = default(Api.GuiThreadInfo);
            guiThreadInfo.cbSize = Marshal.SizeOf(guiThreadInfo);
            if (Api.GetGUIThreadInfo(0,ref guiThreadInfo))
            {
                hWnd = guiThreadInfo.hwndFocus;
            }
            return hWnd;
        }

        [DllImport("kernel32.dll")]
        private static extern int Process32First(IntPtr hSnapshot, ref ProcessEntry32 lppe);

        [DllImport("kernel32.dll")]
        private static extern int Process32Next(IntPtr hSnapshot, ref ProcessEntry32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hSnapshot);

        private const uint TH32CS_SNAPPROCESS = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessEntry32
        {
            public int dwSize;
            public int cntUsage;
            public int th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public int th32ModuleID;
            public int cntThreads;
            public int th32ParentProcessID;
            public int pcPriClassBase;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        public static List<ProcessEntry32> EnumProcesses()
        {
            var ps = new List<ProcessEntry32>();
            IntPtr handle = IntPtr.Zero;
            try
            {
                // Create snapshot of the processes
                handle = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
                ProcessEntry32 pe32 = new ProcessEntry32();
                pe32.dwSize = System.Runtime.InteropServices.
                              Marshal.SizeOf(typeof(ProcessEntry32));

                // Get the first process
                int first = Process32First(handle, ref pe32);
                // If we failed to get the first process, throw an exception
                if (first == 0)
                    throw new Exception("Cannot find first process.");

                // While there's another process, retrieve it
                do
                {
                    IntPtr temp = Marshal.AllocHGlobal((int)pe32.dwSize);
                    Marshal.StructureToPtr(pe32, temp, true);
                    ProcessEntry32 pe = (ProcessEntry32)Marshal.PtrToStructure(temp, typeof(ProcessEntry32));
                    Marshal.FreeHGlobal(temp);
                    ps.Add(pe);
                }
                while (Process32Next(handle, ref pe32) != 0);
            }
            catch
            {
                throw;
            }
            finally
            {
                // Release handle of the snapshot
                CloseHandle(handle);
                handle = IntPtr.Zero;
            }
            return ps;
        }

        public static T GetTopWindow<T>() where T : Window
        {
            var rt = default(T);
            try
            {
                var winds = Application.Current.Windows.OfType<T>();
                if (!winds.xIsNullOrEmpty())
                {
                    var hdict = new Dictionary<int, T>();
                    foreach (T wind in winds)
                    {
                        var hWndSrc = (HwndSource)PresentationSource.FromVisual(wind);
                        int key = (hWndSrc == null) ? 0 : hWndSrc.Handle.ToInt32();
                        hdict[key] = wind;
                    }
                    TraverDesktopHwnd(hWnd =>
                    {
                        if (hdict.ContainsKey(hWnd))
                        {
                            rt = hdict[hWnd];
                        }
                        return rt == null;
                    });
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        public class User32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref User32.RECT lpRect);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hdc);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
        }

        public class GDI32
        {
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, int dwRop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

            public const int CAPTUREBLT = 1073741824;

            public const int SRCCOPY = 13369376;
        }

        public static int GetForegroundWindow()
        {
            return Api.GetForegroundWindow();
        }

        public static List<int> GetChildHwnds(int pHwnd)
        {
            List<int> childHwnds = new List<int>();
            int hwnd = 0;
            while (true)
            {
                hwnd = Api.FindWindowEx(pHwnd, hwnd, null,null);
                if (hwnd == 0)
                {
                    break;
                }
                childHwnds.Add(hwnd);
            }
            return childHwnds;
        }       
    }
}
