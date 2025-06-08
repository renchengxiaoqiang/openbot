using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;

namespace Bot.AssistWindow.NotifyIcon
{
    [ContentProperty("Text")]
    [DefaultEvent("MouseDoubleClick")]
    public class CtlNotifyIcon : FrameworkElement
    {
        public System.Windows.Forms.NotifyIcon NotifyIconInner { get; private set; }
        public static readonly DependencyProperty IconProperty;
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty FormsContextMenuProperty;
        public static RoutedEvent MouseClickEvent { get; set; }
        private bool _isBlink;
        private Icon _oriIcon;
        private Icon _alterIcon;
        private object _blinkSynObj;
        public ImageSource Icon
        {
            get
            {
                return (ImageSource)GetValue(IconProperty);
            }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        public string Text
        {
            get
            {
                return GetValue(TextProperty).ToString();
            }
            set
            {
                NotifyIconInner.Text = value;
                SetValue(TextProperty, value);
            }
        }

        public List<ToolStripItem> MenuItems
        {
            get
            {
                return (List<ToolStripItem>)GetValue(FormsContextMenuProperty);
            }
            set
            {
                SetValue(FormsContextMenuProperty, value);
            }
        }


        public CtlNotifyIcon()
        {
            _isBlink = false;
            _blinkSynObj = new object();
        }

        static CtlNotifyIcon()
        {
            MouseClickEvent = EventManager.RegisterRoutedEvent("MouseClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(CtlNotifyIcon));
            IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CtlNotifyIcon));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CtlNotifyIcon));
            FormsContextMenuProperty = DependencyProperty.Register("MenuItems", typeof(List<ToolStripItem>), typeof(CtlNotifyIcon), new PropertyMetadata(new List<ToolStripItem>()));
        }

        public event MouseButtonEventHandler MouseClick
        {
            add
            {
                AddHandler(MouseClickEvent, value);
            }
            remove
            {
                RemoveHandler(MouseClickEvent, value);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                NotifyIconInner = new System.Windows.Forms.NotifyIcon();
                NotifyIconInner.Icon = ImageToIcon(Icon);
                NotifyIconInner.Visible = Visibility == Visibility.Visible;
                if (!MenuItems.xIsNullOrEmpty())
                {
                    var contextMenuStrip = new ContextMenuStrip();
                    contextMenuStrip.Items.AddRange(MenuItems.ToArray());
                    NotifyIconInner.ContextMenuStrip = contextMenuStrip;
                }
            }
        }



        public void BlinkAsync(ImageSource image)
        {
            Task.Run(() =>
            {
                StartBlink(image);
            });
        }

        public static Icon ImageToIcon(ImageSource imageSrc)
        {
            Icon icon = null;
            if (imageSrc == null) return icon;
            Uri uriResource = new Uri(imageSrc.ToString());
            icon = new Icon(System.Windows.Application.GetResourceStream(uriResource).Stream);
            return icon;
        }

        public void StartBlink(ImageSource imageSrc)
        {
            Task.Run(() =>
            {
                if (_isBlink)
                {
                    StopBlink();
                }
                lock (_blinkSynObj)
                {
                    _isBlink = true;
                    _oriIcon = NotifyIconInner.Icon;
                    _alterIcon = ImageToIcon(imageSrc);
                    while (_isBlink)
                    {
                        Thread.Sleep(500);
                        this.xInvoke(() => { NotifyIconInner.Icon = _alterIcon; });
                        Thread.Sleep(500);
                        this.xInvoke(() => { NotifyIconInner.Icon = _oriIcon; });
                    }
                }

            });
        }

        public void StopBlink()
        {
            _isBlink = false;

        }

        public ToolStripMenuItem CreateItem(string text, EventHandler eventHandler = null, Bitmap image = null, bool enabled = true, object tag = null)
        {
            return new ToolStripMenuItem(text, image, eventHandler)
            {
                Enabled = enabled,
                Tag = tag
            };
        }

        public void InsertItem(int idx, ToolStripItem toolStripItem)
        {
            NotifyIconInner.ContextMenuStrip.Items.Insert(idx, toolStripItem);
        }

        public ToolStripMenuItem GetRootMenu(int idx)
        {
            return NotifyIconInner.ContextMenuStrip.Items[idx] as ToolStripMenuItem;
        }

        public ToolStripSeparator CreateSeparator()
        {
            return new ToolStripSeparator();
        }


        public void RemoveItem(string nick)
        {
            var idx = this.GetFirstLevelItemIndexByTagText(nick);
            if (idx >= 0)
            {
                DispatcherEx.xInvoke(() =>
                {
                    NotifyIconInner.ContextMenuStrip.Items.RemoveAt(idx);
                });
            }
        }

        private int GetFirstLevelItemIndexByTagText(string txt)
        {
            int idx = -1;
            if (NotifyIconInner == null || NotifyIconInner.ContextMenuStrip == null
                || NotifyIconInner.ContextMenuStrip.Items == null)
                return idx;

            for (var i = 0; i < NotifyIconInner.ContextMenuStrip.Items.Count; i++)
            {
                if (NotifyIconInner.ContextMenuStrip.Items[i].Tag as string == txt)
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }
    }

}
