using BotLib.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BotLib.Extensions
{
    public class ClipboardEx
    {
        private static object _synobj = new object();
        public static void UseClipboardWithAutoRestore(Action act)
        {
            if (act != null)
            {
                Dictionary<string, object> bp = null;
                DispatcherEx.xInvoke(() =>
                {
                    bp = Backup(false);
                });
                if (act != null)
                {
                    act();
                }
                DispatcherEx.xInvoke(() =>
                {
                    Restore(bp, false);
                });
            }
        }

        public static void UseClipboardWithAutoRestoreInUiThread(Action act)
        {
            if (act != null)
            {
                lock (_synobj)
                {
                    DispatcherEx.xInvoke(() =>
                    {
                        var dict = Backup(false);
                        if (act != null)
                        {
                            act();
                        }
                        Restore(dict, false);
                    });
                }
            }
        }

        public static Dictionary<string, object> Backup(bool useUiThread = false)
        {
            var dict = new Dictionary<string, object>();
            MaybeInUiThread(() =>
            {
                try
                {
                    IDataObject dataObject = Clipboard.GetDataObject();
                    foreach (var text in dataObject.GetFormats())
                    {
                        dict.Add(text, dataObject.GetData(text));
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }, useUiThread);
            return dict;
        }

        private static void MaybeInUiThread(Action act, bool useUiThread)
        {
            if (useUiThread)
            {
                DispatcherEx.xInvoke(act);
            }
            else if (act != null)
            {
                act();
            }
        }

        public static bool Restore(Dictionary<string, object> dict, bool useUiThread = false)
        {
            bool isok = false;
            MaybeInUiThread(() =>
            {
                try
                {
                    if (!dict.xIsNullOrEmpty())
                    {
                        DataObject dataObject = new DataObject();
                        foreach (string text in dict.Keys)
                        {
                            dataObject.SetData(text, dict[text]);
                        }
                        Clipboard.SetDataObject(dataObject);
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                    isok = true;
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }, useUiThread);
            return isok;
        }

        public static bool SetRichText(string rtf, bool useUiThread = false)
        {
            bool isok = false;
            MaybeInUiThread(() =>
            {
                try
                {
                    Clipboard.SetData(DataFormats.Rtf.ToString(), rtf);
                    isok = true;
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }, useUiThread);
            return isok;
        }

        public static bool SetTextSafe(string txt, bool useUiThread = false, int maxtry = 2, bool tipOnFail = false, bool useUiThreadAfterFail = true)
		{
			bool isok = false;
			string err = null;
			int tryCnt = 0;
			while (tryCnt < maxtry && !isok)
			{
				MaybeInUiThread(()=>
					{
						try
						{
							Clipboard.SetDataObject(txt);
							isok = true;
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
							err = ex.Message;
						}
					}, useUiThread);
				if (!isok)
				{
					Thread.Sleep(10);
					if (useUiThreadAfterFail)
					{
						useUiThread = true;
					}
				}
				tryCnt++;
			}
			if (tipOnFail && !isok)
			{
				MessageBox.Show("无法复制内容到剪贴板！\r\n\r\n出错原因=" + err);
			}
			return isok;
		}

        public static bool GetTextSafe(out string txt, bool useUiThread = false)
        {
            bool isok = false;
            txt = null;
            string tmp = null;
            int tryCnt = 0;
            while (tryCnt < 10 && !isok)
            {
                MaybeInUiThread(() =>
                    {
                        try
                        {
                            tmp = Clipboard.GetText();
                            isok = true;
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }, useUiThread);
                if ( !isok)
                {
                    Thread.Sleep(10);
                }
                tryCnt++;
            }
            txt = tmp;
            return isok;
        }

        [DllImport("user32.dll")]
        public static extern int GetOpenClipboardWindow();

        public static bool IsClipboardBusy()
        {
            return GetOpenClipboardWindow() != 0;
        }

        public static void UseClipboardWithRestore(Action act)
        {
            var preState = Backup(true);
            try
            {
                act();
            }
            finally
            {
                Restore(preState, true);
            }
        }
    }
}
