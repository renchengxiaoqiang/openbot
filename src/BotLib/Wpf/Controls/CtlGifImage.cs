using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;


namespace BotLib.Wpf.Controls
{
    public class CtlGifImage : Image
    {
        private bool _isInitialized;
        private GifBitmapDecoder _gifDecoder;
        private Int32Animation _animation;

        public static readonly DependencyProperty FrameIndexProperty = DependencyProperty.Register("FrameIndex", typeof(int), typeof(CtlGifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));
        public static readonly DependencyProperty AutoStartProperty = DependencyProperty.Register("AutoStart", typeof(bool), typeof(CtlGifImage), new UIPropertyMetadata(false, new PropertyChangedCallback(AutoStartPropertyChanged)));
        public static readonly DependencyProperty GifSourceProperty = DependencyProperty.Register("GifSource", typeof(string), typeof(CtlGifImage), new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(GifSourcePropertyChanged)));

        public int FrameIndex
        {
            get
            {
                return (int)base.GetValue(FrameIndexProperty);
            }
            set
            {
                base.SetValue(FrameIndexProperty, value);
            }
        }

        private void Initialize()
        {
            _gifDecoder = new GifBitmapDecoder(new Uri("pack://application:,,," + GifSource), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _animation = new Int32Animation(0, _gifDecoder.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, _gifDecoder.Frames.Count / 10, (int)(((double)_gifDecoder.Frames.Count / 10.0 - (double)(_gifDecoder.Frames.Count / 10)) * 1000.0))));
            _animation.RepeatBehavior = RepeatBehavior.Forever;
            base.Source = _gifDecoder.Frames[0];
            _isInitialized = true;
        }

        static CtlGifImage()
        {
            UIElement.VisibilityProperty.OverrideMetadata(typeof(CtlGifImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(VisibilityPropertyChanged)));
        }

        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                ((CtlGifImage)sender).StartAnimation();
            }
            else
            {
                ((CtlGifImage)sender).StopAnimation();
            }
        }

        private static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            CtlGifImage ctlGifImage = obj as CtlGifImage;
            ctlGifImage.Source = ctlGifImage._gifDecoder.Frames[(int)ev.NewValue];
        }

        public bool AutoStart
        {
            get
            {
                return (bool)base.GetValue(AutoStartProperty);
            }
            set
            {
                base.SetValue(AutoStartProperty, value);
            }
        }

        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                (sender as CtlGifImage).StartAnimation();
            }
        }

        public string GifSource
        {
            get
            {
                return (string)base.GetValue(GifSourceProperty);
            }
            set
            {
                base.SetValue(GifSourceProperty, value);
            }
        }

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CtlGifImage).Initialize();
        }

        public void StartAnimation()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            base.BeginAnimation(FrameIndexProperty, _animation);
        }

        public void StopAnimation()
        {
            base.BeginAnimation(FrameIndexProperty, null);
        }

    }

}
