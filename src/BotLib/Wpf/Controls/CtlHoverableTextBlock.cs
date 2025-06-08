using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BotLib.Wpf.Controls
{
    public class CtlHoverableTextBlock : TextBlock
    {
        public static readonly DependencyProperty IsClickAndDoubleClickAllUseProperty = DependencyProperty.Register("IsClickAndDoubleClickAllUse", typeof(bool), typeof(CtlHoverableTextBlock), new PropertyMetadata(false));
        public static readonly DependencyProperty IsHoverHighlightProperty = DependencyProperty.Register("IsHoverHighlight", typeof(bool), typeof(CtlHoverableTextBlock), new PropertyMetadata(false));
        private bool _isDoubleClick = false;
        private Brush _defaultForeground;

        public static readonly RoutedEvent ClickEvent;
        public static readonly RoutedEvent DoubleClickEvent;
        public static readonly RoutedEvent RightClickEvent;

        public event RoutedEventHandler Click
        {
            add
            {
                AddHandler(ClickEvent, value);
            }
            remove
            {
                RemoveHandler(ClickEvent, value);
            }
        }

        private void RaiseClickEvent()
        {
            RoutedEventArgs e = new RoutedEventArgs(ClickEvent, this);
            RaiseEvent(e);
        }

        public event RoutedEventHandler DoubleClick
        {
            add
            {
                AddHandler(DoubleClickEvent, value);
            }
            remove
            {
                RemoveHandler(DoubleClickEvent, value);
            }
        }

        private void RaiseDoubleClickEvent()
        {
            RoutedEventArgs e = new RoutedEventArgs(DoubleClickEvent, this);
            RaiseEvent(e);
        }

        public event RoutedEventHandler RightClick
        {
            add
            {
                AddHandler(RightClickEvent, value);
            }
            remove
            {
                RemoveHandler(RightClickEvent, value);
            }
        }

        private void RaiseRightClickEvent()
        {
            var args = new RoutedEventArgs(RightClickEvent, this);
            RaiseEvent(args);
        }

        static CtlHoverableTextBlock()
        {
            ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlHoverableTextBlock));
            DoubleClickEvent = EventManager.RegisterRoutedEvent("DoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlHoverableTextBlock));
            RightClickEvent = EventManager.RegisterRoutedEvent("RightClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlHoverableTextBlock));
        }

        public bool IsClickAndDoubleClickAllUse
        {
            get
            {
                return (bool)GetValue(IsClickAndDoubleClickAllUseProperty);
            }
            set
            {
                SetValue(IsClickAndDoubleClickAllUseProperty, value);
            }
        }

        public bool IsHoverHighlight
        {
            get
            {
                return (bool)GetValue(IsHoverHighlightProperty);
            }
            set
            {
                SetValue(IsHoverHighlightProperty, value);
            }
        }

        public CtlHoverableTextBlock()
        {
            MouseEnter += CtlHoverableTextBlock_MouseEnter;
            MouseLeave += CtlHoverableTextBlock_MouseLeave;
            MouseDown += CtlHoverableTextBlock_MouseDown;
        }

        private void CtlHoverableTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                RaiseRightClickEvent();
            }
            else
            {
                if (IsClickAndDoubleClickAllUse)
                {
                    if (e.ClickCount == 1)
                    {
                        DelayCaller.CallAfterDelayInUIThread(()=>
                        {
                            if (!_isDoubleClick)
                            {
                                RaiseClickEvent();
                            }
                            _isDoubleClick = false;
                        }, 300);
                    }
                    else
                    {
                        if (e.ClickCount == 2)
                        {
                            _isDoubleClick = true;
                            e.Handled = true;
                            RaiseDoubleClickEvent();
                        }
                    }
                }
                else
                {
                    if ( e.ClickCount == 1)
                    {
                        RaiseClickEvent();
                    }
                    else
                    {
                        RaiseDoubleClickEvent();
                    }
                }
            }
        }

        private void CtlHoverableTextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsHoverHighlight)
            {
                Foreground = _defaultForeground;
            }
        }

        private void CtlHoverableTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsHoverHighlight)
            {
                _defaultForeground = Foreground;
                Foreground = Brushes.Red;
            }
        }
    }

}
