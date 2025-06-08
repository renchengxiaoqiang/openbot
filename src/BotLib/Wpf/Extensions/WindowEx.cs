using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace BotLib.Wpf.Extensions
{
    public static class WindowEx
    {
        public static int xHandle(this Window wnd)
        {
            int rt = 0;
            DispatcherEx.xInvoke(()=>
            {
                rt = new WindowInteropHelper(wnd).Handle.ToInt32();
            });
            return rt;
        }

        public static void xSetOwner(this Window wnd, Window owner)
        {
            if (owner != null)
            {
                if (owner != wnd)
                {
                    wnd.Owner = owner;
                }
            }
        }

        public static void xHideToAltTab(this Window wnd)
        {
            wnd.Loaded += Wnd_Loaded4xHideToAltTab;
        }

        private static void Wnd_Loaded4xHideToAltTab(object sender, RoutedEventArgs e)
        {
            Window window = sender as Window;
            window.Loaded -= Wnd_Loaded4xHideToAltTab;
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
            int dwNewLong = (int)GetWindowLong(windowInteropHelper.Handle, GWL_EXSTYLE);
            dwNewLong |= 128;
            SetWindowLong(windowInteropHelper.Handle, -20, (uint)dwNewLong);
        }

        public static void xSetStartUpLocation(this Window wnd, bool startUpCenterOwner)
        {
            if (startUpCenterOwner)
            {
                if (wnd.Owner != null)
                {
                    wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            else
            {
                wnd.SourceInitialized += Wnd_SourceInitialized;
            }
        }

        private static void Wnd_SourceInitialized(object sender, EventArgs e)
        {
            Window window = sender as Window;
            if (window != null)
            {
                window.SourceInitialized -= Wnd_SourceInitialized;
                AdjustStartUpLocation(window);
            }
        }

        public static void xMoveToWorkAreaCenter(this Window wnd)
        {
            if (wnd.IsLoaded)
            {
                double left = (SystemParameters.WorkArea.Width - wnd.ActualWidth) / 2.0;
                if (left < 0.0)
                {
                    left = 5.0;
                }
                double top = (SystemParameters.WorkArea.Height - wnd.ActualHeight) / 2.0;
                if (top < 0.0)
                {
                    top = 0.0;
                }
                wnd.Left = left;
                wnd.Top = top;
            }
        }

        public static void xKeepWindowFullVisibleAtResize(this Window wnd)
        {
            wnd.SizeChanged += Wnd_SizeChanged;
        }

        private static void Wnd_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Window window = sender as Window;
            if (e.WidthChanged)
            {
                double actualWidth = window.ActualWidth;
                if (window.Left + actualWidth > SystemParameters.VirtualScreenWidth)
                {
                    double left = SystemParameters.VirtualScreenWidth - actualWidth - 10.0;
                    if (left < 0.0)
                    {
                        left = 5.0;
                    }
                    window.Left = left;
                }
            }
            if (e.HeightChanged)
            {
                double actualHeight = window.ActualHeight;
                if (window.Top + actualHeight > SystemParameters.WorkArea.Height)
                {
                    double top = SystemParameters.WorkArea.Height - actualHeight - 10.0;
                    if (top < 0.0)
                    {
                        top = 5.0;
                    }
                    window.Top = top;
                }
            }
        }

        public static void AdjustStartUpLocation(Window wnd)
        {
            Point point = wnd.PointToScreen(Mouse.GetPosition(wnd)).xConvertPhisicalToLogical();
            double actualWidth = wnd.ActualWidth;
            double actualHeight = wnd.ActualHeight;
            if (point.X <= 0.0 && point.Y <= 0.0)
            {
                wnd.Left = (SystemParameters.WorkArea.Width - actualWidth) / 2.0;
                wnd.Top = (SystemParameters.WorkArea.Height - actualHeight) / 2.0;
            }
            else
            {
                if (point.Y + 12.0 + actualHeight > SystemParameters.WorkArea.Height)
                {
                    double showTop = point.Y - 12.0 - actualHeight;
                    if (showTop > 0.0)
                    {
                        point.Y = showTop;
                    }
                    else
                    {
                        point.Y = SystemParameters.WorkArea.Height - actualHeight - 10.0;
                    }
                }
                else
                {
                    point.Y += 12.0;
                }
                
                if (point.Y < 0.0)
                {
                    point.Y = 0.0;
                }
                double showLeft = point.X - actualWidth / 2.0;
                if (showLeft + actualWidth + 10.0 > SystemParameters.WorkArea.Width)
                {
                    showLeft = SystemParameters.WorkArea.Width - 10.0 - actualWidth;
                }
                if (showLeft < 0.0)
                {
                    showLeft = 10.0;
                }
                wnd.Left = showLeft;
                wnd.Top = point.Y;
            }
        }

        public static void xShowFirstTime(this Window wnd)
        {
            wnd.Show();
            wnd.xBrintTop();
        }

        public static void xBrintTop(this Window wnd)
        {
            if (!wnd.Topmost)
            {
                wnd.Topmost = true;
                DelayCaller.CallAfterDelayInUIThread(()=>
                {
                    wnd.Topmost = false;
                    wnd.Activate();
                }, 500);
            }
        }

        public static void xReShow(this Window wnd)
        {
            if (wnd.WindowState == WindowState.Minimized)
            {
                wnd.WindowState = WindowState.Normal;
            }
            wnd.xShowFirstTime();
        }

        public static void xSetOwner(this Window wnd, int hwnd)
        {
            if (wnd != null)
            {
                WindowInteropHelper windowInteropHelper = new WindowInteropHelper(wnd);
                windowInteropHelper.Owner = new IntPtr(hwnd);
            }
        }

        public static bool SetNativeWindowInWPFWindowAsChild(IntPtr hWndNative, Window window)
        {
            IntPtr hWndNewParent = hWndNative;
            hWndNative = new WindowInteropHelper(window).Handle;
            uint windowLong = GetWindowLong(hWndNative, GWL_STYLE);
            SetWindowLong(hWndNative, GWL_STYLE, windowLong | WS_CHILD);
            IntPtr value = SetParent(hWndNative, hWndNewParent);
            return value != IntPtr.Zero;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        public static T GetFirstShowingWindow<T>() where T : Window
		{
            foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd is T)
				{
					return wnd as T;
				}
			}
			return default(T);
		}

        public static bool HasShowingWindowOtherThan<T>(T wnd) where T : Window
		{
			foreach (Window window in Application.Current.Windows)
			{
				if (window is T && window != wnd)
				{
					return true;
				}
			}
			return false;
		}

        public static List<T> GetAppWindows<T>() where T : Window
		{
			List<T> wnds = new List<T>();
			foreach (Window window in Application.Current.Windows)
			{
				if (window is T)
				{
					wnds.Add(window as T);
				}
			}
			return wnds;
		}

        public static bool xIsModal(this Window window)
        {
            return (bool)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
        }

        private static int GWL_STYLE = -16;
        private static int GWL_EXSTYLE = -20;
        private static uint WS_CHILD = 1073741824U;
        private static uint WS_POPUP = 2147483648U;
        private static uint WS_CAPTION = 12582912U;
        private static uint WS_THICKFRAME = 262144U;
        private static uint WS_EX_DLGMODALFRAME = 1U;
        private static uint WS_EX_WINDOWEDGE = 256U;
        private static uint WS_EX_CLIENTEDGE = 512U;
        private static uint WS_EX_STATICEDGE = 131072U;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        [Flags]
        public enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 128
        }

        public enum GetWindowLongFields
        {
            GWL_EXSTYLE = -20
        }

        private struct RECT
        {
            public int left;
            public int top;
            public int right;

            public int bottom;
        }

        [Flags]
        private enum SetWindowPosFlags : uint
        {
            SWP_ASYNCWINDOWPOS = 16384U,
            SWP_DEFERERASE = 8192U,
            SWP_DRAWFRAME = 32U,
            SWP_FRAMECHANGED = 32U,
            SWP_HIDEWINDOW = 128U,
            SWP_NOACTIVATE = 16U,
            SWP_NOCOPYBITS = 256U,
            SWP_NOMOVE = 2U,
            SWP_NOOWNERZORDER = 512U,
            SWP_NOREDRAW = 8U,
            SWP_NOREPOSITION = 512U,
            SWP_NOSENDCHANGING = 1024U,
            SWP_NOSIZE = 1U,
            SWP_NOZORDER = 4U,
            SWP_SHOWWINDOW = 64U
        }
    }
}
