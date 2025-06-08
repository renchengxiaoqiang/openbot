using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BotLib.Wpf.Extensions;

namespace BotLib.Wpf.Controls
{
    public class CtlRichTextBox : RichTextBox
    {

        private DelayCaller _dcaller;
        private bool _hasUnhandledInput = false;
        private bool _isImeProcessed = false;

        public static readonly RoutedEvent EvCaretMoveByHumanEvent;

        static CtlRichTextBox()
        {
            EvCaretMoveByHumanEvent = EventManager.RegisterRoutedEvent("EvCaretMoveByHuman", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlRichTextBox));
        }

        public event RoutedEventHandler EvCaretMoveByHuman
        {
            add
            {
                base.AddHandler(EvCaretMoveByHumanEvent, value);
            }
            remove
            {
                base.RemoveHandler(EvCaretMoveByHumanEvent, value);
            }
        }

        protected RoutedEventArgs RaiseEvCaretMoveByHumanEvent()
        {
            var e = new RoutedEventArgs(EvCaretMoveByHumanEvent, this);
            RaiseEvent(e);
            return e;
        }

        public CtlRichTextBox()
        {
            InitDelayCaller();
        }

        public string Text
        {
            get
            {
                return this.GetText(true);
            }
            set
            {
                this.SetText(value);
            }
        }

        private void InitDelayCaller()
        {
            _dcaller = new DelayCaller(()=>
            {
                if (_hasUnhandledInput && Keyboard.FocusedElement == this)
                {
                    _hasUnhandledInput = false;
                    if (!_isImeProcessed || this.GetTextBeforeCaret(1) != " ")
                    {
                        RaiseEvCaretMoveByHumanEvent();
                    }
                }
            }, 100, true);
        }

        public void ClearErrorMark()
        {
            this.GetDocumentRange().ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }

        public void ShowErrorMark(SyntaxErrorException err)
        {
            TextRange range = this.GetRange(err.StartIndex, err.Length);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            _hasUnhandledInput = true;
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            _dcaller.CallAfterDelay();
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            _hasUnhandledInput = true;
            _isImeProcessed = false;
            _dcaller.CallAfterDelay();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            _isImeProcessed = (e.Key == Key.ImeProcessed);
            if (!_isImeProcessed)
            {
                _hasUnhandledInput = true;
            }
        }

        public class SyntaxErrorException : Exception
        {
            public string SourceText { get; private set; }
            public string Error { get; private set; }
            public int StartIndex { get; private set; }
            public int Length { get; private set; }

            public SyntaxErrorException(string source, string err, int startIndex, int length)
            {
                SourceText = source;
                Error = err;
                StartIndex = startIndex;
                Length = length;
            }

            public override string Message
            {
                get
                {
                    return Error;
                }
            }
        }
    }
}
