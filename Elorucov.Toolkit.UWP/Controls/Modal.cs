using Elorucov.Toolkit.UWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Elorucov.Toolkit.UWP.Controls {

    [TemplateVisualState(Name = "Bottom", GroupName = "HeightStates")]
    [TemplateVisualState(Name = "Center", GroupName = "HeightStates")]
    [TemplateVisualState(Name = "Stretch", GroupName = "HeightStates")]
    public partial class Modal : ContentControl, IModal {

        #region Private fields

        Popup popup;
        bool IsWide = false;
        bool WasShowed = false;

        Grid LayoutRoot;
        Border Layer;
        Border AnimationBorder;
        Grid DialogWrapper;
        Border ModalContent;
        Rectangle ShadowBorder;
        TextBlock TitleText;
        Button CloseButton;

        DropShadow _shadow;
        SpriteVisual _visual;

        const int animationDuration = 250;

        #endregion

        #region Properties

        public static readonly DependencyProperty FullSizeDesiredProperty =
            DependencyProperty.Register("FullSizeDesired", typeof(bool), typeof(Modal), new PropertyMetadata(default(bool)));

        public bool FullSizeDesired {
            get { return (bool)GetValue(FullSizeDesiredProperty); }
            set { SetValue(FullSizeDesiredProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(Modal), new PropertyMetadata(0d));

        public double CornerRadius {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty DropShadowProperty =
            DependencyProperty.Register("DropShadow", typeof(bool), typeof(Modal), new PropertyMetadata(default(bool)));

        public bool DropShadow {
            get { return (bool)GetValue(DropShadowProperty); }
            set { SetValue(DropShadowProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Modal), new PropertyMetadata(""));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(Modal), new PropertyMetadata(default(Visibility)));

        public Visibility CloseButtonVisibility {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        #endregion

        public Modal() {
            this.DefaultStyleKey = typeof(Modal);
            popup = new Popup();
            popup.Child = this;
            RegisterPropertyChangedCallback(FullSizeDesiredProperty, (a, b) => { if (WasShowed) Resize(); });
            RegisterPropertyChangedCallback(MaxWidthProperty, (a, b) => { if (WasShowed) Resize(); });
            RegisterPropertyChangedCallback(CornerRadiusProperty, ChangeShadowCornerRadius);
            RegisterPropertyChangedCallback(MaxWidthProperty, ChangeMaxWidth);
            RegisterPropertyChangedCallback(MarginProperty, ChangeMargin);
            RegisterPropertyChangedCallback(DropShadowProperty, (a, b) => {
                if (_shadow != null) _shadow.Color = DropShadow ? Colors.Black : Colors.Transparent;
            });
            RegisterPropertyChangedCallback(TitleProperty, (a, b) => {
                if (TitleText != null) TitleText.Visibility = String.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;
            });
        }

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();

            LayoutRoot = (Grid)GetTemplateChild("LayoutRoot");
            Layer = (Border)GetTemplateChild("Layer");
            AnimationBorder = (Border)GetTemplateChild("AnimationBorder");
            DialogWrapper = (Grid)GetTemplateChild("DialogWrapper");
            ShadowBorder = (Rectangle)GetTemplateChild("ShadowBorder");
            ModalContent = (Border)GetTemplateChild("ModalContent");
            TitleText = (TextBlock)GetTemplateChild("TitleText");
            CloseButton = (Button)GetTemplateChild("CloseButton");

            ModalContent.SizeChanged += (a, b) => {
                ShadowBorder.Height = ModalContent.ActualHeight;
                if(_visual != null) {
                    _visual.Size = ModalContent.RenderSize.ToVector2();
                    _shadow.Mask = ShadowBorder.GetAlphaMask();
                }
            };

            Resize();
            SetupShadow();

            Window.Current.SizeChanged += (a, b) => Resize(b.Size);
            CloseButton.Click += (a, b) => Hide();
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += (a, b) => Resize();
            Loaded += (a, b) => {
                if (TitleText != null) TitleText.Visibility = String.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;
                ChangeMaxWidth(this, MaxWidthProperty);
                ChangeMargin(this, MarginProperty);
                UpdateCornerRadius(CornerRadius);
                Animate(Windows.UI.Composition.AnimationDirection.Normal, animationDuration);
                SetFocus(ModalContent);
                EventHandler<BackRequestedEventArgs> bre = new EventHandler<BackRequestedEventArgs>(HideModal);
            };
            WasShowed = true;
        }

        private void HideModal(object sender, BackRequestedEventArgs e) {
            e.Handled = true;
            Hide();
        }

        public event EventHandler<object> Closed;

        public void Show() {
            popup.IsOpen = true;
        }

        public async void Hide(object data = null) {
            Animate(Windows.UI.Composition.AnimationDirection.Reverse, animationDuration);
            await Task.Delay(animationDuration);
            Closed?.Invoke(this, data);
            popup.IsOpen = false;
        }

        #region Internal functions and events

        private void SetFocus(DependencyObject element) {
            if (element == null) return;
            
            Control ctrl = element as Control;
            if(ctrl != null) {
                ctrl.Focus(FocusState.Programmatic);
            } else {
                int count = VisualTreeHelper.GetChildrenCount(element);
                for (int i = 0; i < count; i++) {
                    DependencyObject elem = VisualTreeHelper.GetChild(element, i);
                    SetFocus(elem);
                }
            }
        }

        private void Resize() => Resize(new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height));

        private void Resize(Size size) {
            popup.Width = size.Width;
            popup.Height = size.Height;
            Width = size.Width;
            Height = size.Height;
            if(FullSizeDesired) {
                VisualStateManager.GoToState(this, "Stretch", true);
            } else {
                VisualStateManager.GoToState(this, size.Width > ModalContent.MaxWidth ? "Center" : "Bottom", true);
            }
            IsWide = size.Width > ModalContent.MaxWidth;
            FixDialogWrapper();
        }

        private void FixDialogWrapper() {
            double top = 0;
            double bottom = 0;
            if(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") {
                if(DisplayInformation.GetForCurrentView().CurrentOrientation == DisplayOrientations.Portrait) {
                    top = StatusBar.GetForCurrentView().OccludedRect.Height;
                    bottom = Window.Current.Bounds.Height - ApplicationView.GetForCurrentView().VisibleBounds.Height - top;
                } else {
                    top = 16; bottom = 16;
                }
            } else {
                top = 32;
                bottom = IsWide ? 32 : 0;
            }
            DialogWrapper.Margin = new Thickness(0, top, 0, bottom);
        }

        private void ChangeShadowCornerRadius(DependencyObject sender, DependencyProperty dp) {
            double r = (double)GetValue(dp);
            UpdateCornerRadius(r);
        }

        bool dontUpdateMaxWidth = false;
        private void ChangeMaxWidth(DependencyObject sender, DependencyProperty dp) {
            double mw = (double)GetValue(dp);
            if (!dontUpdateMaxWidth) {
                if (ModalContent != null) {
                    ModalContent.MaxWidth = mw;
                    ShadowBorder.MaxWidth = mw;
                    dontUpdateMaxWidth = true;
                    MaxWidth = Double.MaxValue;
                    dontUpdateMaxWidth = false;
                }
            }
        }

        bool dontUpdateMargin = false;
        private void ChangeMargin(DependencyObject sender, DependencyProperty dp) {
            Thickness t = (Thickness)GetValue(dp);
            if (!dontUpdateMargin) {
                if (ModalContent != null) {
                    ModalContent.Margin = t;
                    ShadowBorder.Margin = t;
                    dontUpdateMargin = true;
                    Margin = new Thickness(0);
                    dontUpdateMargin = false;
                }
            }
        }

        private void UpdateCornerRadius(double r) {
            if(ModalContent != null && ShadowBorder != null) {
                ModalContent.CornerRadius = new CornerRadius(r);
                ShadowBorder.RadiusX = r;
                ShadowBorder.RadiusY = r;
                _shadow.Mask = DropShadow ? ShadowBorder.GetAlphaMask() : null;
            }
        }

        private void SetupShadow() {
            var compositor = ElementCompositionPreview.GetElementVisual(ModalContent).Compositor;
            _visual = compositor.CreateSpriteVisual();
            _visual.Size = ModalContent.RenderSize.ToVector2();
            _visual.Offset = new Vector3(0, 0, 0);

            _shadow = compositor.CreateDropShadow();
            _shadow.Offset = new Vector3(0, 8, 0);
            _shadow.BlurRadius = 32;
            _shadow.Color = DropShadow ? Colors.Black : Colors.Transparent;
            _shadow.Opacity = 0.5f;
            _shadow.Mask = ShadowBorder.GetAlphaMask();
            _visual.Shadow = _shadow;
            ElementCompositionPreview.SetElementChildVisual(ShadowBorder, _visual);
        }

        private void Animate(Windows.UI.Composition.AnimationDirection direction, int duration) {
            ElementCompositionPreview.SetIsTranslationEnabled(AnimationBorder, true);
            Visual dvisual = ElementCompositionPreview.GetElementVisual(AnimationBorder);
            Compositor dcompositor = dvisual.Compositor;

            Vector3KeyFrameAnimation vfa = dcompositor.CreateVector3KeyFrameAnimation();

            dvisual.Offset = new Vector3(0, (float)ModalContent.ActualHeight, 0);
            vfa.InsertKeyFrame(1f, new Vector3(0, 0, 0));
            vfa.Duration = TimeSpan.FromMilliseconds(duration);
            vfa.Direction = direction;
            vfa.IterationCount = 1;

            Visual lvisual = ElementCompositionPreview.GetElementVisual(Layer);
            Compositor lcompositor = lvisual.Compositor;
            lvisual.Opacity = 0;

            ScalarKeyFrameAnimation sfa = lcompositor.CreateScalarKeyFrameAnimation();
            sfa.InsertKeyFrame(1, 1);
            sfa.Duration = TimeSpan.FromMilliseconds(duration);
            sfa.Direction = direction;
            sfa.IterationCount = 1;

            dvisual.Opacity = IsWide ? 0 : 1; 

            ScalarKeyFrameAnimation sdfa = dcompositor.CreateScalarKeyFrameAnimation();
            sdfa.InsertKeyFrame(1, 1);
            sdfa.Duration = TimeSpan.FromMilliseconds(duration);
            sdfa.Direction = direction;
            sdfa.IterationCount = 1;

            dvisual.StartAnimation("Offset", vfa);
            lvisual.StartAnimation("Opacity", sfa);
            dvisual.StartAnimation("Opacity", sdfa);
        }

        #endregion
    }
}
