using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BotLib.Wpf.Extensions;

namespace BotLib.Wpf.InteliSense
{
    public partial class WndInteliSence : Window
    {
        public event RoutedPropertyChangedEventHandler<InteliSenseData> EvItemSelected;
        public event RoutedPropertyChangedEventHandler<InteliSenseData> EvItemFocused;
        public static readonly RoutedEvent EvItemSelectedEvent;
        public static readonly RoutedEvent EvItemFocusedEvent;

        protected RoutedPropertyChangedEventArgs<InteliSenseData> RaiseEvItemSelectedEvent(InteliSenseData data)
        {
            if (data == null) return null;
            RoutedPropertyChangedEventArgs<InteliSenseData> routedPropertyChangedEventArgs = new RoutedPropertyChangedEventArgs<InteliSenseData>(null, data);
            routedPropertyChangedEventArgs.RoutedEvent = EvItemSelectedEvent;
            RaiseEvent(routedPropertyChangedEventArgs);
            return routedPropertyChangedEventArgs;
        }

        protected RoutedPropertyChangedEventArgs<InteliSenseData> RaiseEvItemFocusedEvent(InteliSenseData data)
        {
            RoutedPropertyChangedEventArgs<InteliSenseData> routedPropertyChangedEventArgs = new RoutedPropertyChangedEventArgs<InteliSenseData>(null, data);
            routedPropertyChangedEventArgs.RoutedEvent = EvItemFocusedEvent;
            base.RaiseEvent(routedPropertyChangedEventArgs);
            return routedPropertyChangedEventArgs;
        }

        private List<InteliSenseData> CollA;
        private List<InteliSenseData> CollB;
        private TextBoxBase _editor;
        private bool _toolTipShowAtRight = true;


        static WndInteliSence()
        {
            EvItemSelectedEvent = EventManager.RegisterRoutedEvent("EvItemSelected", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<InteliSenseData>), typeof(WndInteliSence));
            EvItemFocusedEvent = EventManager.RegisterRoutedEvent("EvItemFocused", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<InteliSenseData>), typeof(WndInteliSence));
        }
        public WndInteliSence(TextBoxBase editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.PreviewKeyDown += _editor_PreviewKeyDown;
            LostKeyboardFocus += WndInteliSence_LostKeyboardFocus;
            var owner = editor.xFindParentWindow();
            if (owner.IsLoaded)
            {
                if (owner != this)
                {
                    Owner = owner;
                }
            }
            else
            {
                owner.Loaded += (sender, e) =>
                {
                    if (owner != this)
                    {
                        Owner = owner;
                    }
                };
            }
            owner.LocationChanged += (sender, e) =>
            {
                if (IsVisible)
                {
                    Hide();
                }
            };
        }

        public void MyShow(List<InteliSenseData> data, List<InteliSenseData> commands = null)
        {
            if (!HasData(data, commands))
            {
                Hide();
            }
            else
            {
                InitListBox(data, commands);
                if (Visibility > Visibility.Visible)
                {
                    Show();
                }
                UpdateLocation();
                _editor.Focus();
            }
        }

        private bool HasData(List<InteliSenseData> data, List<InteliSenseData> commands)
        {
            return (data != null && data.Count > 0) || (commands != null && commands.Count > 0);
        }

        private void InitListBox(List<InteliSenseData> data, List<InteliSenseData> commands)
        {
            lstContent.Items.Clear();
            foreach (var inteliSenseData in data)
            {
                var it = new ListBoxItem();
                it.Content = inteliSenseData.Text;
                it.Tag = inteliSenseData;
                lstContent.Items.Add(it);
            }
            if (data.Count > 0 && commands.Count > 0)
            {
                var it = new ListBoxItem();
                it.IsEnabled = false;
                it.Background = Brushes.LightGray;
                it.Content = null;
                it.Tag = null;
                lstContent.Items.Add(it);
            }
            foreach (var dt in commands)
            {
                var it = new ListBoxItem();
                it.Content = dt.Text;
                it.Tag = dt;
                lstContent.Items.Add(it);
            }
        }

        private void WndInteliSence_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement != _editor && !this.xContainsDescendant(Keyboard.FocusedElement))
            {
                Hide();
            }
        }

        private void _editor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsVisible) return;
            Key key = e.Key;
            if (key != Key.Escape)
            {
                if (key == Key.Down)
                {
                    if (lstContent.Items.Count > 0)
                    {
                        Activate();
                        lstContent.Focus();
                        if (lstContent.SelectedItem == null)
                        {
                            lstContent.SelectedIndex = 0;
                        }
                        var selectedItem = lstContent.GetSelectedListBoxItem();
                        if (selectedItem != null)
                        {
                            selectedItem.BringIntoView();
                            selectedItem.Focusable = true;
                            selectedItem.Focus();
                        }
                        e.Handled = true;
                    }
                }
            }
            else
            {
                Hide();
            }

        }

        private void UpdateLocation()
        {
            Rect caretRect = GetCaretRect();
            Point inteliSenseListLeftTop = GetInteliSenseListLeftTop(caretRect.Location, caretRect.Height);
            if (NeedUpdateLocation(inteliSenseListLeftTop))
            {
                AdjustToolTipLocation(inteliSenseListLeftTop);
                bool toolTipShowAtRight = _toolTipShowAtRight;
                if (toolTipShowAtRight)
                {
                    Left = inteliSenseListLeftTop.X;
                    Top = inteliSenseListLeftTop.Y;
                }
                else
                {
                    Left = inteliSenseListLeftTop.X - cvs.Width;
                    Top = inteliSenseListLeftTop.Y;
                }
            }
        }

        private void AdjustToolTipLocation(Point listLocation)
        {
            if (listLocation.X + lstContent.ActualWidth + cvs.Width > SystemParameters.VirtualScreenWidth)
            {
                _toolTipShowAtRight = false;
                Grid.SetColumn(cvs, 0);
                Canvas.SetRight(tbk, 2.0);
            }
            else
            {
                _toolTipShowAtRight = true;
                Grid.SetColumn(cvs, 2);
                Canvas.SetLeft(tbk, 2.0);
            }
        }

        private Rect GetCaretRect()
        {
            Rect rect;
            if (_editor is TextBox)
            {
                TextBox textBox = _editor as TextBox;
                rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
            }
            else
            {
                RichTextBox richTextBox = _editor as RichTextBox;
                rect = richTextBox.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
            }
            rect.Location = _editor.PointToScreenLogical(rect.Location);
            return rect;
        }

        private bool NeedUpdateLocation(Point lt)
        {
            return lt.Y != base.Top || lt.X < base.Left || lt.X - base.Left > 10.0 || (_toolTipShowAtRight && lt.X + base.ActualWidth > SystemParameters.VirtualScreenWidth);
        }

        private Point GetInteliSenseListLeftTop(Point lefttop, double lineheight)
        {
            double x = lefttop.X;
            double y = lefttop.Y + lineheight;
            if (y + ActualHeight > SystemParameters.WorkArea.Height)
            {
                y = lefttop.Y - ActualHeight;
            }
            if (x + lstContent.ActualWidth > SystemParameters.VirtualScreenWidth)
            {
                x = SystemParameters.VirtualScreenWidth - ActualWidth;
            }
            return new Point(x, y);
        }

        private void SelectAnItem()
        {
            if (lstContent.SelectedItem != null)
            {
                var item = lstContent.SelectedItem as ListBoxItem;
                RaiseEvItemSelectedEvent(((item != null) ? item.Tag : null) as InteliSenseData);
            }
            Hide();
        }

        private void lstContent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectAnItem();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key != Key.Return)
            {
                if (key != Key.Escape)
                {
                    switch (key)
                    {
                        case Key.Prior:
                            return;
                        case Key.Next:
                            return;
                        case Key.Left:
                        case Key.Right:
                            BackToEditor(e);
                            return;
                        case Key.Up:
                            {
                                if (lstContent.SelectedIndex == 0)
                                {
                                    BackToEditor(e);
                                }
                                return;
                            }
                        case Key.Down:
                            return;
                    }
                    _editor.Focus();
                }
                else
                {
                    Hide();
                }
            }
            else
            {
                e.Handled = true;
                SelectAnItem();
            }
        }

        private void BackToEditor(KeyEventArgs e)
        {
            e.Handled = true;
            Hide();
            _editor.Focus();
        }

        private void lstContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstContent.SelectedItem == null)
            {
                tbk.Visibility = Visibility.Collapsed;
                return;
            }
            var it = lstContent.SelectedItem as ListBoxItem;
            var dt = ((it != null) ? it.Tag : null) as InteliSenseData;
            if (dt != null)
            {
                RaiseEvItemFocusedEvent(dt);
                if (string.IsNullOrEmpty(dt.ToolTip))
                {
                    tbk.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var selectedIt = lstContent.GetSelectedListBoxItem();
                    if (selectedIt != null)
                    {
                        tbk.Text = dt.ToolTip;
                        tbk.Visibility = Visibility.Visible;
                        tbk.UpdateLayout();
                        Point point = selectedIt.LocationInContainer(this);
                        point.Y += 2.0;
                        double len = Math.Min(point.Y, base.ActualHeight - tbk.ActualHeight - 7.0);
                        if (len < 0.0)
                        {
                            len = 0.0;
                        }
                        Canvas.SetTop(tbk, len);
                    }
                }
            }
        }

        private void lstContent_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                lstContent.SelectedItem = null;
            }
        }

    }
}
