using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Elorucov.Toolkit.UWP.Controls {
    public sealed class MediaSlider : Control {
        public static readonly DependencyProperty ThumbBackgroundProperty =
               DependencyProperty.Register("ThumbBackground", typeof(Brush), typeof(MediaSlider), new PropertyMetadata(default(Brush)));

        public Brush ThumbBackground {
            get { return (Brush)GetValue(ThumbBackgroundProperty); }
            set { SetValue(ThumbBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
               DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(MediaSlider), new PropertyMetadata(default(TimeSpan)));

        public TimeSpan Duration {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
               DependencyProperty.Register("Position", typeof(TimeSpan), typeof(MediaSlider), new PropertyMetadata(default(TimeSpan)));

        public TimeSpan Position {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public event EventHandler<TimeSpan> PositionChanged;

        #region Private fields

        Grid Root;
        Border PosLine;
        Border BufLine;
        Border DurLine;
        Ellipse Thumb;
        Border PointerArea;
        Border PositionFlyout;
        TextBlock PositionTime;

        bool isPressing = false;

        #endregion

        public MediaSlider() {
            this.DefaultStyleKey = typeof(MediaSlider);
            RegisterPropertyChangedCallback(DurationProperty, (a, b) => { SetupSlider(); });
            RegisterPropertyChangedCallback(PositionProperty, (a, b) => { SetupSlider(); });
        }

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();
            Root = (Grid)GetTemplateChild("Root");
            PosLine = (Border)GetTemplateChild("PosLine");
            BufLine = (Border)GetTemplateChild("BufLine");
            DurLine = (Border)GetTemplateChild("DurLine");
            Thumb = (Ellipse)GetTemplateChild("Thumb");
            PointerArea = (Border)GetTemplateChild("PointerArea");
            PositionFlyout = (Border)GetTemplateChild("PositionFlyout");
            PositionTime = (TextBlock)GetTemplateChild("PositionTime");

            Root.SizeChanged += SetupSlider;
            PointerArea.PointerPressed += StartDragThumb;
            Unloaded += (a, b) => {
                Root.SizeChanged -= SetupSlider;
                PointerArea.PointerPressed -= StartDragThumb;
            };
        }

        private void StartDragThumb(object sender, PointerRoutedEventArgs e) {
            isPressing = true;
            ChangeThumbPosition(e.GetCurrentPoint(PointerArea).Position.X);
            Window.Current.Content.PointerMoved += Delta;
            Window.Current.Content.PointerReleased += StopDragThumb;
        }

        private void Delta(object sender, PointerRoutedEventArgs e) {
            if (isPressing) {
                e.Handled = true;
                double x = e.GetCurrentPoint(PointerArea).Position.X;
                ChangeThumbPosition(x);
            }
        }

        private void StopDragThumb(object sender, PointerRoutedEventArgs e) {
            Window.Current.Content.PointerMoved -= Delta;
            Window.Current.Content.PointerReleased -= StopDragThumb;
            double d = Duration.TotalMilliseconds;
            double w = Root.ActualWidth;
            double sp = Canvas.GetLeft(Thumb);
            double t = Thumb.Width;
            double p = d / (w - t) * sp;
            Position = TimeSpan.FromMilliseconds(p);
            PositionChanged?.Invoke(this, Position);
            isPressing = false;
            PositionFlyout.Visibility = Visibility.Collapsed;
        }

        private void ChangeThumbPosition(double x) {
            double w = Root.ActualWidth;
            double t = Thumb.Width;
            double plt = 0;

            var z = x - (t / 2);
            if (z >= 0 && z <= w - t) {
                plt = z;
            } else if (z < 0) {
                plt = 0;
            } else if (z > w - t) {
                plt = w - t;
            }
            Canvas.SetLeft(Thumb, plt);

            double d = Duration.TotalMilliseconds;
            double pt = d / (w - t) * plt;
            TimeSpan tm = TimeSpan.FromMilliseconds(pt);
            ChangePosFlyoutPosition(tm, x);
            PositionFlyout.Visibility = Visibility.Visible;
        }

        private void ChangePosFlyoutPosition(TimeSpan ts, double x) {
            PositionTime.Text = ts.ToString(ts.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");

            double w = Root.ActualWidth;
            double t = PositionFlyout.ActualWidth;
            double p = 0;
            var z = x - (t / 2);
            if (z >= 0 && z <= w - t) {
                p = z;
            } else if (z < 0) {
                p = 0;
            } else if (z > w - t) {
                p = w - t;
            }
            Canvas.SetLeft(PositionFlyout, p);
        }

        private void SetupSlider(object a = null, object b = null) {
            if(Root != null) {
                double w = Root.ActualWidth;
                double d = Duration.TotalMilliseconds;
                double p = Position.TotalMilliseconds;
                double pl = w / d * p;
                PosLine.Width = pl;

                if(!isPressing) {
                    double t = Thumb.Width;
                    double plt = (w - t) / d * p;
                    Canvas.SetLeft(Thumb, plt);
                }
            }
        }
    }
}
