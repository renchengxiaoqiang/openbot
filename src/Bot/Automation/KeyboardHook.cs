using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bot.Automation
{
    public class KeyboardHook
    {
        private static int hKeyboardHook;
        private HookProc KeyboardHookProcedure;
        public KeyEventHandler KeyDownEvent;
        public KeyPressEventHandler KeyPressEvent;
        public KeyEventHandler KeyUpEvent;
        public const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 256;
        private const int WM_KEYUP = 257;
        private const int WM_SYSKEYDOWN = 260;
        private const int WM_SYSKEYUP = 261;
        static KeyboardHook()
        {
            hKeyboardHook = 0;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        ~KeyboardHook()
        {
            Stop();
        }

        [DllImport("Kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] IpKeyState);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern short GetKeyState(int nVrtKey);

        [DllImport("kernel32.dll")]
        protected static extern IntPtr GetModuleHandle(string name);


        public void Stop()
        {
            bool retKeyboard = true;
            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            if (!retKeyboard)
            {
                throw new Exception("卸载钩子失败！");
            }
        }

        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
            {
                KeyboardHookStruct keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                if (KeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    var vkCode = (Keys)keyboardHookStruct.vkCode;
                    var e = new KeyEventArgs(vkCode);
                    KeyDownEvent(this, e);
                }
                if (KeyPressEvent != null && wParam == WM_KEYDOWN)
                {
                    byte[] IpKeyState = new byte[256];
                    GetKeyboardState(IpKeyState);
                    byte[] lpChar = new byte[2];
                    if (ToAscii(keyboardHookStruct.vkCode, keyboardHookStruct.scanCode, IpKeyState, lpChar, keyboardHookStruct.flags) == 1)
                    {
                        var e = new KeyPressEventArgs((char)lpChar[0]);
                        KeyPressEvent(this, e);
                    }
                }
                if (KeyUpEvent != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    var vkCode = (Keys)keyboardHookStruct.vkCode;
                    var e = new KeyEventArgs(vkCode);
                    KeyUpEvent(this, e);
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        public void Start()
        {
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("安装键盘钩子失败");
                }
            }
        }

        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpKeyState, byte[] lpChar, int uFlags);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(int hhk);

        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
    }
}
