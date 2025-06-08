using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using BotLib;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Bot.Automation.ChatDeskNs;
using System.Windows;
using Bot.Automation;

namespace Bot.ChromeNs
{
    public class QNRpa
    {

        private DateTime _preUpdateChatBrowserRectTime;
        private DateTime _preSendPlainTextAndImageTime;
        private BitmapImage _preSendPlainTextAndImageImage;
        public DateTime LatestSetTextTime;

        private AutomationElement _sendMessageButton;
        private AutomationElement _closeContactButton;
        private TextBox _messageInputTextArea;

        private FlaUI.Core.Application automationApplication;
        private UIA3Automation uia3Automation;

        public string LastSetPlainText
        {
            get;
            private set;
        }

        private QN _qn;

        public QNRpa(QN qn)
        {
            _qn = qn;
            automationApplication = FlaUI.Core.Application.Attach(Desk.Inst.ProcessId);
            uia3Automation = new UIA3Automation();
            UpdateChatBrowserRect();
        }


        public async void UpdateChatBrowserRect()
        {
            if (Desk.Inst.IsVisibleAndNotMinimized)
            {
                if ((DateTime.Now - _preUpdateChatBrowserRectTime).TotalSeconds >= 3)
                {
                    _preUpdateChatBrowserRectTime = DateTime.Now;
                    if (automationApplication.MainWindowHandle.ToInt32() < 1) return;
                    await Task.Run(() =>
                    {
                        try
                        {
                            var topWnds = automationApplication.GetAllTopLevelWindows(uia3Automation);
                            var mainWnd = topWnds.FirstOrDefault(k => k.ClassName == "MutilChatView");
                            if (mainWnd == null) return;

                            var descendants = mainWnd.FindAllDescendants();
                            var sendMessageButton = descendants.FirstOrDefault(k =>
                            {
                                if (k.Properties.Name.IsSupported && k.Name == "发送")
                                {
                                    return true;
                                }
                                return false;
                            });
                            _sendMessageButton = sendMessageButton;

                            //var closeContactButton = descendants.FirstOrDefault(k =>
                            //{

                            //    if (k.Properties.Name.IsSupported && k.Name == "关闭")
                            //    {
                            //        return true;
                            //    }
                            //    return false;
                            //});
                            //_closeContactButton = closeContactButton;

                            var messageInputTextArea = descendants.FirstOrDefault(k =>
                            {
                                if (k.Properties.ClassName.IsSupported && k.ClassName == "TextRichEdit")
                                {
                                    return true;
                                }
                                return false;
                            });
                            _messageInputTextArea = messageInputTextArea.AsTextBox();
                        }
                        catch
                        {

                        }
                    });

                }
            }


        }

        public async Task SendImageAsync(string buyer, string imagePath)
        {
            await Task.Run(() =>
            {
                var image = BitmapImageEx.CreateFromFile(imagePath);
                OpenAndSendImage(buyer, image);
            });
        }

        private bool OpenAndSendImage(string buyer, BitmapImage image)
        {
            bool sendResult = false;
            if (_qn.Buyer == null || _qn.Buyer.Nick != buyer)
            {
                _qn.OpenChat(buyer);
                Thread.Sleep(500);
                Util.WaitFor(() => _qn.Buyer.Nick == buyer, 5000, 10, false);
            }
            if (_qn.Buyer.Nick == buyer)
            {
                if (!Desk.Inst.IsVisible)
                {
                    Desk.Inst.Show();
                    Util.WaitFor(new Func<bool>(() => Desk.Inst.IsVisible), 3000, 10, false);
                }
                SetAndSendImage(image);
            }
            sendResult = true;
            return sendResult;
        }

        private bool SetAndSendImage(BitmapImage image)
        {
            bool rt = false;
            if ((DateTime.Now - _preSendPlainTextAndImageTime).TotalSeconds < 1.1
                && _preSendPlainTextAndImageImage == image)
            {
                rt = false;
            }
            else
            {
                _preSendPlainTextAndImageTime = DateTime.Now;
                _preSendPlainTextAndImageImage = image;
                if (SetImage(image))
                {
                    if (_sendMessageButton == null)
                    {
                        UpdateChatBrowserRect();
                    }
                    _sendMessageButton.Click();
                    //WinApi.PressEnter();
                    rt = true;
                }
                else
                {
                    rt = false;
                }
            }
            return rt;
        }

        private bool SetImage(BitmapImage img)
        {
            bool isok = false;
            ClipboardEx.UseClipboardWithAutoRestoreInUiThread(() =>
            {
                FocusEditor();
                Clipboard.Clear();
                Clipboard.SetImage(img);
                WinApi.PressCtrlV();
                DateTime now = DateTime.Now;
                do
                {
                    if (!string.IsNullOrEmpty(_messageInputTextArea.Text))
                    {
                        isok = true;
                        break;
                    }
                    DispatcherEx.DoEvents();
                } while ((DateTime.Now - now).TotalSeconds < 2.0);
                Util.WriteTimeElapsed(now, "等待时间");
            });
            return isok;
        }

        public bool FocusEditor()
        {
            bool isok = false;
            DispatcherEx.xInvoke(() =>
            {
                Desk.Inst.BringTop();
                try
                {
                    var point = _messageInputTextArea.GetClickablePoint();
                    FlaUI.Core.Input.Mouse.Click(new System.Drawing.Point { X = point.X + 5, Y = point.Y + 5 });
                    isok = true;
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            });
            return isok;
        }

        public async Task SendTextAsync(string buyer, string text)
        {
            await OpenAndSendText(buyer, text);
        }

        private async Task<bool> OpenAndSendText(string buyer, string text)
        {
            bool sendResult = false;
            if (_qn.Buyer == null || _qn.Buyer.Nick != buyer)
            {
                _qn.OpenChat(buyer);
                await Task.Delay(500);
                await _qn.GetCurrentConversationID();
            }
            if (_qn.Buyer.Nick == buyer)
            {
                if (!Desk.Inst.IsVisible)
                {
                    Desk.Inst.Show();
                    Util.WaitFor(new Func<bool>(() => Desk.Inst.IsVisible), 3000, 10, false);
                }
                //SetAndSendText(text);

                _qn.InsertText2Inputbox(buyer, text);
                Thread.Sleep(500);
                if (_sendMessageButton == null)
                {
                    UpdateChatBrowserRect();
                }
                _sendMessageButton.Click();
            }
            sendResult = true;
            return sendResult;
        }


        private bool SetAndSendText(string text)
        {
            var isok = false;
            try
            {
                //_messageInputTextArea.Text = text;
                //await Task.Delay(200);

                //ClipboardEx.UseClipboardWithAutoRestoreInUiThread(() =>
                //{
                //    FocusEditor();
                //    Clipboard.Clear();
                //    ClipboardEx.SetTextSafe(text);
                //    WinApi.PressCtrlV();
                //    DateTime now = DateTime.Now;
                //    do
                //    {
                //        if (!_qn.IsInputboxEmpty().GetAwaiter().GetResult())
                //        {
                //            isok = true;
                //            break;
                //        }
                //        DispatcherEx.DoEvents();
                //    } while ((DateTime.Now - now).TotalSeconds < 2.0);
                //    Util.WriteTimeElapsed(now, "等待时间");
                //});
                //var point = _sendMessageButton.GetClickablePoint();

                //_qn.InsertText2Inputbox(text);

                //_sendMessageButton.Click();
                //WinApi.PressEnter();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return isok;
        }

    }
}
