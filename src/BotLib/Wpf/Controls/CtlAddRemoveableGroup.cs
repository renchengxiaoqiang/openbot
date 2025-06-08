using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BotLib.Extensions;

namespace BotLib.Wpf.Controls
{
    public class CtlAddRemoveableGroup : ScrollViewer
    {
        private StackPanel _spanel = new StackPanel();
        private int _visibleRows = -1;
        private double _rowHeight;
        private SolidColorBrush _editorBackground;
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CtlAddRemoveableGroup), new PropertyMetadata("", new PropertyChangedCallback(CtlAddRemoveableGroup.TitleChangedCallback)));

        public int VisibleRows
        {
            get
            {
                return _visibleRows;
            }
            set
            {
                if (value != _visibleRows)
                {
                    _visibleRows = value;
                    base.MaxHeight = _rowHeight * (double)value;
                }
            }
        }

        public CtlAddRemoveableGroup()
        {
            base.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddChild(_spanel);
            AddCtl("", null);
            base.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _rowHeight = base.DesiredSize.Height;
            VisibleRows = 10;
        }

        public SolidColorBrush EditorBackground
        {
            get
            {
                return _editorBackground;
            }
            set
            {
                _editorBackground = value;
                foreach (CtlAddRemoveable ctlAddRemoveable in _spanel.Children)
                {
                    if (ctlAddRemoveable != null)
                    {
                        ctlAddRemoveable.rtb.Background = _editorBackground;
                    }
                }
            }
        }

        public void Init(IEnumerable<string> slist)
        {
            _spanel.Children.Clear();
            if (slist != null)
            {
                foreach (string text in slist)
                {
                    AddCtl(text, null);
                }
            }
        }

        public string Title
        {
            get
            {
                return (string)GetValue(CtlAddRemoveableGroup.TitleProperty);
            }
            set
            {
                SetValue(CtlAddRemoveableGroup.TitleProperty, value);
            }
        }

        public new bool HasContent
        {
            get
            {
                List<string> texts = Texts;
                bool hasContent;
                if (texts == null || texts.Count == 0)
                {
                    hasContent = false;
                }
                else
                {
                    foreach (string s in texts)
                    {
                        if (!s.xIsNullOrEmptyOrSpace())
                        {
                            return true;
                        }
                    }
                    hasContent = false;
                }
                return hasContent;
            }
        }

        public List<string> Texts
        {
            get
            {
                List<string> list = new List<string>();
                for (int i = 0; i < _spanel.Children.Count; i++)
                {
                    CtlAddRemoveable ctlAddRemoveable = _spanel.Children[i] as CtlAddRemoveable;
                    if (ctlAddRemoveable != null)
                    {
                        list.Add(ctlAddRemoveable.Text.Trim());
                    }
                }
                return list;
            }
            set
            {
                _spanel.Children.Clear();
                foreach (string text in value)
                {
                    CtlAddRemoveable ctlAddRemoveable = AddCtl("", null);
                    ctlAddRemoveable.Text = text;
                }
            }
        }

        private static void TitleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CtlAddRemoveableGroup ctlAddRemoveableGroup = d as CtlAddRemoveableGroup;
            if (ctlAddRemoveableGroup != null)
            {
                ctlAddRemoveableGroup.UpdateTitle();
            }
        }

        public event RoutedEventHandler EvRemove
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvRemoveEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvRemoveEvent, value);
            }
        }

        public event RoutedEventHandler EvAdd
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvAddEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvAddEvent, value);
            }
        }

        public event RoutedEventHandler EvIntoEditMode
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvIntoEditModeEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvIntoEditModeEvent, value);
            }
        }

        public event RoutedEventHandler EvExitEditMode
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvExitEditModeEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvExitEditModeEvent, value);
            }
        }

        public event CtlAddRemoveable.RoutedValidateEventHandler EvValidate
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvValidateEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvValidateEvent, value);
            }
        }

        public event RoutedEventHandler EvSubmit
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvSubmitEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvSubmitEvent, value);
            }
        }


        public bool HasError()
        {
            foreach (object uie in _spanel.Children)
            {
                CtlAddRemoveable ctlAddRemoveable = uie as CtlAddRemoveable;
                if (ctlAddRemoveable.HasError())
                {
                    return true;
                }
            }
            return false;
        }

        private CtlAddRemoveable AddCtl(string text = "", object insertAfter = null)
        {
            CtlAddRemoveable ctlAddRemoveable = new CtlAddRemoveable(Title);
            ctlAddRemoveable.EvAdd += Ctl_EvAdd;
            ctlAddRemoveable.EvRemove += Ctl_EvRemove;
            ctlAddRemoveable.Margin = new Thickness(5.0);
            ctlAddRemoveable.Text = text;
            if (EditorBackground != null)
            {
                ctlAddRemoveable.rtb.Background = EditorBackground;
            }
            if (insertAfter == null)
            {
                _spanel.Children.Add(ctlAddRemoveable);
            }
            else
            {
                int index = _spanel.Children.IndexOf(insertAfter as UIElement) + 1;
                _spanel.Children.Insert(index, ctlAddRemoveable);
            }
            UpdateTitle();
            return ctlAddRemoveable;
        }

        private void UpdateTitle()
        {
            for (int i = 0; i < _spanel.Children.Count; i++)
            {
                CtlAddRemoveable ctlAddRemoveable = _spanel.Children[i] as CtlAddRemoveable;
                if (ctlAddRemoveable != null)
                {
                    ctlAddRemoveable.UpdateTitleWithIndex(i + 1, Title);
                }
            }
        }

        private void Ctl_EvRemove(object sender, RoutedEventArgs e)
        {
            RemoveCtl(sender);
        }

        private void RemoveCtl(object sender)
        {
            if (sender != null)
            {
                _spanel.Children.Remove(sender as UIElement);
                if (_spanel.Children.Count == 0)
                {
                    AddCtl("", null);
                }
                UpdateTitle();
            }
        }

        private void Ctl_EvAdd(object sender, RoutedEventArgs e)
        {
            AddCtl("", sender);
        }

        public void FocusEx()
        {
            if (_spanel.Children.Count > 0)
            {
                CtlAddRemoveable ctlAddRemoveable = _spanel.Children[0] as CtlAddRemoveable;
                ctlAddRemoveable.FocusEx();
            }
        }

    }

}
