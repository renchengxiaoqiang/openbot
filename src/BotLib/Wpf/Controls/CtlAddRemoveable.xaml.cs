using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using BotLib.Misc;
using BotLib.Wpf.Extensions;

namespace BotLib.Wpf.Controls
{
	public partial class CtlAddRemoveable : UserControl
    {
        private string _title;
        private DelayCaller _delayValidater;
        private string _preTextChangedText;
        private readonly double _lineHeight = 20.0;
        private readonly int _showLinesAtFocus = 5;
        private CtlRichTextBox.SyntaxErrorException _errDesc = null;
        public static readonly DependencyProperty TitleProperty;
        private bool _isAddButtonEnabled = true;
        private bool _isRemoveButtonEnabled = true;
        private string _preText = "";
        private bool _isEditMode = false;
        public delegate void RoutedValidateEventHandler(object sender, RoutedValidateEventArgs args);


        public static readonly RoutedEvent EvAddEvent;
        public static readonly RoutedEvent EvRemoveEvent;
        public static readonly RoutedEvent EvIntoEditModeEvent;
        public static readonly RoutedEvent EvExitEditModeEvent;
        public static readonly RoutedEvent EvValidateEvent;
        public static readonly RoutedEvent EvSubmitEvent;

        static CtlAddRemoveable()
        {
            EvAddEvent = EventManager.RegisterRoutedEvent("EvAdd", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlAddRemoveable));
            EvRemoveEvent = EventManager.RegisterRoutedEvent("EvRemove", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlAddRemoveable));
            EvIntoEditModeEvent = EventManager.RegisterRoutedEvent("EvIntoEditMode", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlAddRemoveable));
            EvExitEditModeEvent = EventManager.RegisterRoutedEvent("EvExitEditMode", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlAddRemoveable));
            EvValidateEvent = EventManager.RegisterRoutedEvent("EvValidate", RoutingStrategy.Bubble, typeof(RoutedValidateEventHandler), typeof(CtlAddRemoveable));
            EvSubmitEvent = EventManager.RegisterRoutedEvent("EvSubmit", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlAddRemoveable));
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CtlAddRemoveable), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTitleChanged)));
        }

		public CtlAddRemoveable(string title = "")
		{
			InitializeComponent();
			_title = title;
			Height = _lineHeight;
			rtb.TextChanged += Rtb_TextChanged;
			rtb.EvCaretMoveByHuman += Rtb_EvCaretMoveByHuman;
			_delayValidater = new DelayCaller(new Action(ValidateOnTextChanged), 500, true);
		}

		public string Text
		{
			get
			{
				return rtb.Text;
			}
			set
			{
				rtb.Text = value;
			}
		}

		private void Rtb_EvCaretMoveByHuman(object sender, RoutedEventArgs e)
		{
			tbkCharIndex.Text = "字：" + rtb.GetTextBeforeCaret(0).Length;
		}

		private void ValidateOnTextChanged()
		{
            var text = rtb.GetText(true);
			if (text != _preTextChangedText)
			{
				_preTextChangedText = text;
				RaiseEvValidateEvent();
			}
		}

		public void FocusEditor()
		{
			rtb.Focus();
		}

		private void Rtb_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SyntaxError != null)
			{
				_delayValidater.CallAfterDelay();
			}
		}

		public CtlRichTextBox.SyntaxErrorException SyntaxError
		{
			get
			{
				return _errDesc;
			}
			set
			{
				_errDesc = value;
				if (value == null)
				{
					tbkError.Visibility = Visibility.Collapsed;
					rtb.ClearErrorMark();
				}
				else
				{
					tbkError.Text = value.Error;
					tbkError.Visibility = Visibility.Visible;
					rtb.ShowErrorMark(value);
				}
			}
		}

		public bool HasError()
		{
			return SyntaxError != null;
		}

		public CtlRichTextBox RichTextBox
		{
			get
			{
				return rtb;
			}
		}

		public event RoutedEventHandler EvAdd
		{
			add
			{
				AddHandler(EvAddEvent, value);
			}
			remove
			{
				RemoveHandler(EvAddEvent, value);
			}
		}

		protected RoutedEventArgs RaiseEvAddEvent()
		{
			var args = new RoutedEventArgs();
			args.RoutedEvent = EvAddEvent;
			args.Source = this;
			RaiseEvent(args);
			return args;
		}

		public event RoutedEventHandler EvRemove
		{
			add
			{
				AddHandler(EvRemoveEvent, value);
			}
			remove
			{
				RemoveHandler(EvRemoveEvent, value);
			}
		}

		protected RoutedEventArgs RaiseEvRemoveEvent()
		{
			var args = new RoutedEventArgs();
			args.RoutedEvent = EvRemoveEvent;
			RaiseEvent(args);
			return args;
		}

		public event RoutedEventHandler EvIntoEditMode
		{
			add
			{
				AddHandler(EvIntoEditModeEvent, value);
			}
			remove
			{
				RemoveHandler(EvIntoEditModeEvent, value);
			}
		}

		protected RoutedEventArgs RaiseEvIntoEditModeEvent()
		{
			var args = new RoutedEventArgs();
			args.RoutedEvent = EvIntoEditModeEvent;
			RaiseEvent(args);
			return args;
		}

		public event RoutedEventHandler EvExitEditMode
		{
			add
			{
				AddHandler(EvExitEditModeEvent, value);
			}
			remove
			{
				RemoveHandler(EvExitEditModeEvent, value);
			}
		}

		protected RoutedEventArgs RaiseEvExitEditModeEvent()
		{
			var args = new RoutedEventArgs();
			args.RoutedEvent = EvExitEditModeEvent;
			RaiseEvent(args);
			return args;
		}

		public event RoutedValidateEventHandler EvValidate
		{
			add
			{
				AddHandler(EvValidateEvent, value);
			}
			remove
			{
				RemoveHandler(EvValidateEvent, value);
			}
		}

		protected RoutedValidateEventArgs RaiseEvValidateEvent()
		{
			var args = new RoutedValidateEventArgs();
			args.RoutedEvent = EvValidateEvent;
			RaiseEvent(args);
			SyntaxError = args.SyntaxErrorExp;
			return args;
		}

		internal void FocusEx()
		{
			rtb.Focus();
		}

		public event RoutedEventHandler EvSubmit
		{
			add
			{
				AddHandler(EvSubmitEvent, value);
			}
			remove
			{
				RemoveHandler(EvSubmitEvent, value);
			}
		}

		protected RoutedEventArgs RaiseEvSubmitEvent()
		{
			var args = new RoutedEventArgs();
			args.RoutedEvent = EvSubmitEvent;
			RaiseEvent(args);
			return args;
		}

		private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs baseValue)
		{
			var ctlAddRemoveable = d.xFindAncestorFromMe<CtlAddRemoveable>();
			if (ctlAddRemoveable != null)
			{
                ctlAddRemoveable.btnAdd.ToolTip = "新增 " + ctlAddRemoveable._title;
                ctlAddRemoveable.btnRemove.ToolTip = "删除此 " + ctlAddRemoveable._title;
			}
		}

		public void UpdateTitleWithIndex(int idx, string title)
		{
			_title = title;
			tbkTitle.Text = _title + idx + "：";
		}

		public bool IsAddButtonEnabled
		{
			get
			{
				return _isAddButtonEnabled;
			}
			set
			{
				if (value != _isAddButtonEnabled)
				{
					btnAdd.IsEnabled = value;
					_isAddButtonEnabled = value;
				}
			}
		}

		public bool IsRemoveButtonEnabled
		{
			get
			{
				return _isRemoveButtonEnabled;
			}
			set
			{
				if (_isRemoveButtonEnabled != value)
				{
					_isRemoveButtonEnabled = value;
					btnRemove.IsEnabled = value;
				}
			}
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			IntoEditMode();
		}

		private void IntoEditMode()
		{
			if (!_isEditMode)
			{
				_isEditMode = true;
				_preText = rtb.GetText(true);
				Height = _lineHeight * (double)_showLinesAtFocus;
				spAddRemoveButtons.Visibility = Visibility.Collapsed;
				spRow1.Visibility = Visibility.Visible;
				tbkError.Visibility = Visibility.Collapsed;
				RaiseEvIntoEditModeEvent();
			}
		}

		private void ExitEditMode()
		{
            if (_isEditMode)
			{
				_isEditMode = false;
				Height = _lineHeight;
				spRow1.Visibility = Visibility.Collapsed;
				spAddRemoveButtons.Visibility = Visibility.Visible;
				RaiseEvExitEditModeEvent();
			}
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var focusedElement = Keyboard.FocusedElement;
			if (focusedElement != btnOk && focusedElement != btnCancel)
			{
				bool isEditMode = _isEditMode;
				if (isEditMode)
				{
					Submit();
				}
			}
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			RaiseEvAddEvent();
		}

		private void btnRemove_Click(object sender, RoutedEventArgs e)
		{
			RaiseEvRemoveEvent();
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			Submit();
		}

		private void Submit()
		{
			RoutedValidateEventArgs routedValidateEventArgs = RaiseEvValidateEvent();
			if (SyntaxError == null)
			{
				ExitEditMode();
				RaiseEvSubmitEvent();
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			rtb.SetText(_preText);
			ExitEditMode();
		}

		public class RoutedValidateEventArgs : RoutedEventArgs
		{
			public string ErrorDesc { get; set; }
			public CtlRichTextBox.SyntaxErrorException SyntaxErrorExp { get; set; }
		}
	}
}
