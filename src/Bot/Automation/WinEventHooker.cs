using BotLib;
using BotLib.BaseClass;
using BotLib.Wpf.Extensions;
using Bot.AssistWindow;
using Bot.Automation.ChatDeskNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bot.Automation
{
    public class WinEventHooker : Disposable
    {
        private List<int> _instHookHandles;
        public static EventHandler<WinEventHookEventArgs> EvForegroundChanged;
        public EventHandler<WinEventHookEventArgs> EvFocused;
        public EventHandler<WinEventHookEventArgs> EvTextChanged;
        public EventHandler<WinEventHookEventArgs> EvMinimizeStart;
        public EventHandler<WinEventHookEventArgs> EvLocationChanged;
        private readonly int _threadId;
        private readonly int _hwnd;
        private static readonly WinApiLocal.WinEventDelegate DlgWinEventStaticCallback;
        public static int PreForegroundHwnd;
        public static int CurForegroundHwnd;
        private static Dictionary<int, string> _hwndDesc;
        private readonly WinApiLocal.WinEventDelegate DlgWinEventCallback;
        private static readonly WinApiLocal.WinEventDelegate DlgTestCallback;
        private static WinEventHooker _hook;

		public WinEventHooker(int threadId, int hwnd)
		{
			_instHookHandles = new List<int>();
			_threadId = threadId;
			_hwnd = hwnd;
            DlgWinEventCallback = new WinApiLocal.WinEventDelegate(WinEventStatic);
			DispatcherEx.xBeginInvoke(InitHooks);
		}

		static WinEventHooker()
		{
			DlgWinEventStaticCallback = new WinApiLocal.WinEventDelegate(WinEventStaticCallback);
			_hwndDesc = new Dictionary<int, string>();
			DispatcherEx.xBeginInvoke(()=>
			{
                SetWinEventHook();
			});
		}

        private static void SetWinEventHook()
		{
			WinApiLocal.SetWinEventHook(EventTypeEnum.EVENT_SYSTEM_FOREGROUND, EventTypeEnum.EVENT_SYSTEM_FOREGROUND, 0, DlgWinEventStaticCallback, 0, 0, 0u);
		}

		private static void WinEventStaticCallback(int hWinEventHook, uint eventType, int hwnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
		{
			try
			{
				if (Application.Current != null)
				{
					PreForegroundHwnd = CurForegroundHwnd;
					CurForegroundHwnd = hwnd;
                    if (EvForegroundChanged != null)
					{
                        EvForegroundChanged(null, new WinEventHookEventArgs
						{
							Hwnd = hwnd,
							EvtType = (EventTypeEnum)eventType,
							ThreadId = dwEventThread,
							Timestamp = dwmsEventTime
						});
					}
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

        private void WinEventStatic(int hWinEventHook, uint eventType, int hwnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
		{
            DispatcherEx.xInvoke(() => WinEventCallbackInner(hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwmsEventTime));
		}

		private void WinEventCallbackInner(int hWinEventHook, uint eventType, int hwnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
		{
			try
			{
				if (!IsDisposed)
				{
					if (Application.Current == null)
					{
						Dispose();
                        return;
					}
					var args = new WinEventHookEventArgs
					{
						Hwnd = hwnd,
						EvtType = (EventTypeEnum)eventType,
						Timestamp = dwmsEventTime
					};
					EventTypeEnum evtType = args.EvtType;
					if (evtType <= EventTypeEnum.EVENT_OBJECT_FOCUS)
					{
						if (evtType != EventTypeEnum.EVENT_SYSTEM_MINIMIZESTART)
						{
							if (evtType == EventTypeEnum.EVENT_OBJECT_FOCUS)
							{
                                if (EvFocused != null)
								{
                                    EvFocused(this, args);
								}
							}
						}
						else
						{
                            if (EvMinimizeStart != null)
							{
                                EvMinimizeStart(this, args);
							}
						}
					}
					else if (evtType != EventTypeEnum.EVENT_OBJECT_LOCATIONCHANGE)
					{
						if (evtType == EventTypeEnum.EVENT_OBJECT_VALUECHANGE)
						{
                            if (EvTextChanged != null)
							{
                                EvTextChanged(this, args);
							}
						}
					}
					else if (hwnd == _hwnd)
					{
                        if (EvLocationChanged != null)
						{
                            EvLocationChanged(this, args);
						}
					}
				}
			}
			catch (Exception e)
			{
                Log.Exception(e);
			}
		}

		private void SetHook(EventTypeEnum eventMin, EventTypeEnum eventMax = EventTypeEnum.Unknown)
		{
			if (eventMax == EventTypeEnum.Unknown)
			{
				eventMax = eventMin;
			}
			int item = WinApiLocal.SetWinEventHook(eventMin, eventMax, 0, DlgWinEventCallback, 0, _threadId, 0u);
			_instHookHandles.Add(item);
		}

		protected override void CleanUp_UnManaged_Resources()
		{
			try
			{
				DispatcherEx.xInvoke(()=>{
                    
			        foreach (var handle in _instHookHandles)
			        {
				        WinApiLocal.UnhookWinEvent(handle);
			        }
			        _instHookHandles.Clear();
                });
			}
			catch (Exception e)
			{
                Log.Exception(e);
			}
		}

		protected override void CleanUp_Managed_Resources()
		{
		}

		private void InitHooks()
		{
			SetHook(EventTypeEnum.EVENT_OBJECT_FOCUS, EventTypeEnum.Unknown);
			SetHook(EventTypeEnum.EVENT_OBJECT_VALUECHANGE, EventTypeEnum.Unknown);
			SetHook(EventTypeEnum.EVENT_SYSTEM_MINIMIZESTART, EventTypeEnum.Unknown);
			SetHook(EventTypeEnum.EVENT_OBJECT_LOCATIONCHANGE, EventTypeEnum.Unknown);
		}


		public enum EventTypeEnum
		{
			Unknown,
			EVENT_SYSTEM_FOREGROUND = 3,
			EVENT_OBJECT_FOCUS = 32773,
			EVENT_OBJECT_VALUECHANGE = 32782,
			EVENT_SYSTEM_MINIMIZESTART = 22,
			EVENT_OBJECT_LOCATIONCHANGE = 32779
		}

		public class WinEventHookEventArgs : EventArgs
		{
			public int Hwnd;

			public EventTypeEnum EvtType;

			public int ThreadId;

			public int Timestamp;
		}

		private static class WinApiLocal
		{
			[DllImport("user32.dll")]
			public static extern int SetWinEventHook(EventTypeEnum eventMin, EventTypeEnum eventMax, int hmodWinEventProc, WinApiLocal.WinEventDelegate pfnWinEventProc, int idProcess, int idThread, uint dwFlags);

			[DllImport("user32.dll")]
			public static extern bool UnhookWinEvent(int hWinEventHook);

			public const uint EVENT_SYSTEM_FOREGROUND = 3u;

			public const uint WINEVENT_OUTOFCONTEXT = 0u;

			private const uint WINEVENT_INCONTEXT = 4u;

			private const uint EVENT_OBJECT_NAMECHANGE = 32780u;

			private const uint EVENT_OBJECT_LOCATIONCHANGE = 32779u;

			private const uint EVENT_OBJECT_CONTENTSCROLLED = 32789u;

			private const uint EVENT_OBJECT_FOCUS = 32773u;

			private const uint EVENT_OBJECT_HIDE = 32771u;

			private const uint EVENT_OBJECT_INVOKED = 32787u;

			private const uint EVENT_OBJECT_STATECHANGE = 32778u;

			private const uint EVENT_OBJECT_VALUECHANGE = 32782u;

			private const uint EVENT_SYSTEM_DESKTOPSWITCH = 32u;

			private const uint EVENT_SYSTEM_DIALOGSTART = 16u;

			private const uint EVENT_SYSTEM_MINIMIZESTART = 22u;

			private const uint EVENT_SYSTEM_MINIMIZEEND = 23u;

			private const uint EVENT_SYSTEM_SCROLLINGSTART = 18u;

			private const uint EVENT_SYSTEM_SCROLLINGEND = 19u;

			public delegate void WinEventDelegate(int hWinEventHook, uint eventType, int hwnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);
		}
	}

}
