using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
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

        public static readonly DependencyProperty BufferingProgressProperty =
               DependencyProperty.Register("BufferingProgress", typeof(double), typeof(MediaSlider), new PropertyMetadata((double)1));

        public double BufferingProgress {
            get { return (double)GetValue(BufferingProgressProperty); }
            set { SetValue(BufferingProgressProperty, value); }
        }

        public UIElement DragStopHandlerElement { get; set; } = Window.Current.Content;

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
            RegisterPropertyChangedCallback(BufferingProgressProperty, (a, b) => { SetupSlider(); });
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
            DragStopHandlerElement.PointerMoved += Delta;
            DragStopHandlerElement.PointerReleased += StopDragThumb;
        }

        private void Delta(object sender, PointerRoutedEventArgs e) {
            if (isPressing) {
                e.Handled = true;
                double x = e.GetCurrentPoint(PointerArea).Position.X;
                if (x >= 0 && x <= PointerArea.ActualWidth) {
                    ChangeThumbPosition(x);
                } else {
                    StopDragThumb();
                }
            }
        }

        private void StopDragThumb(object sender, PointerRoutedEventArgs e) {
            StopDragThumb();
        }

        private void StopDragThumb() {
            DragStopHandlerElement.PointerMoved -= Delta;
            DragStopHandlerElement.PointerReleased -= StopDragThumb;
            isPressing = false;

            double d = Duration.TotalMilliseconds;
            double w = Root.ActualWidth;
            double sp = Canvas.GetLeft(Thumb);
            double t = Thumb.Width;
            double p = d / (w - t) * sp;
            if (1 / d * p <= BufferingProgress) {
                Position = TimeSpan.FromMilliseconds(p);
                PositionChanged?.Invoke(this, Position);
            } else {
                SetupSlider();
            }
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
            try {
                if (Root != null) {
                    double w = Root.ActualWidth;
                    double d = Duration.TotalMilliseconds;
                    double p = Position.TotalMilliseconds;
                    if (d > 0) {
                        Thumb.Visibility = Visibility.Visible;
                        double pl = w / d * p;
                        PosLine.Width = pl;

                        if (!isPressing) {
                            double t = Thumb.Width;
                            double plt = (w - t) / d * p;
                            Canvas.SetLeft(Thumb, plt);
                        }
                    } else {
                        Thumb.Visibility = Visibility.Collapsed;
                    }
                    if (BufferingProgress >= 0 && BufferingProgress <= 1) {
                        BufLine.Width = w / 1 * BufferingProgress;
                    } else if (BufferingProgress < 0) {
                        BufLine.Width = 0;
                    } else if (BufferingProgress > 1) {
                        BufLine.Width = w;
                    }
                }
            } catch (Exception ex) {
                Debug.WriteLine($"SetupSlider error: (0x{ex.HResult.ToString("x8")}): {ex.Message.Trim()}");
            }
        }
    }
}
