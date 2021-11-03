using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
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
        ButtonBase CloseButton; // чтобы можно было заюзать HyperlinkButton в кастомных шаблонах

        DropShadow _shadow;
        SpriteVisual _visual;
        bool CanUseThemeShadow { get { return ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8); } }

        const int animationDuration = 250;

        #endregion

        #region Properties

        public static readonly DependencyProperty FullSizeDesiredProperty =
            DependencyProperty.Register(nameof(FullSizeDesired), typeof(bool), typeof(Modal), new PropertyMetadata(default(bool)));

        public bool FullSizeDesired {
            get { return (bool)GetValue(FullSizeDesiredProperty); }
            set { SetValue(FullSizeDesiredProperty, value); }
        }

        public static readonly DependencyProperty CornersRadiusProperty =
            DependencyProperty.Register(nameof(CornersRadius), typeof(double), typeof(Modal), new PropertyMetadata(0d));

        public double CornersRadius {
            get { return (double)GetValue(CornersRadiusProperty); }
            set { SetValue(CornersRadiusProperty, value); }
        }

        public static readonly DependencyProperty ElevationLevelProperty =
            DependencyProperty.Register(nameof(ElevationLevel), typeof(ushort), typeof(Modal), new PropertyMetadata((ushort)128));

        public ushort ElevationLevel {
            get { return (ushort)GetValue(ElevationLevelProperty); }
            set { SetValue(ElevationLevelProperty, value); }
        }

        public static readonly DependencyProperty DropShadowProperty =
            DependencyProperty.Register(nameof(DropShadow), typeof(bool), typeof(Modal), new PropertyMetadata(true));

        public bool DropShadow {
            get { return (bool)GetValue(DropShadowProperty); }
            set { SetValue(DropShadowProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Modal), new PropertyMetadata(""));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register(nameof(CloseButtonVisibility), typeof(Visibility), typeof(Modal), new PropertyMetadata(default(Visibility)));

        public Visibility CloseButtonVisibility {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty LayerBrushProperty =
            DependencyProperty.Register(nameof(LayerBrush), typeof(Brush), typeof(Modal), new PropertyMetadata(default(Brush)));

        public Brush LayerBrush {
            get { return (Brush)GetValue(LayerBrushProperty); }
            set { SetValue(LayerBrushProperty, value); }
        }

        #endregion

        public Modal() {
            this.DefaultStyleKey = typeof(Modal);
            CheckOverrideStyles();
            popup = new Popup();
            popup.Child = this;
            RegisterPropertyChangedCallback(FullSizeDesiredProperty, (a, b) => { if (WasShowed) Resize(); });
            RegisterPropertyChangedCallback(MaxWidthProperty, (a, b) => { if (WasShowed) Resize(); });
            RegisterPropertyChangedCallback(CornersRadiusProperty, ChangeShadowCornersRadius);
            RegisterPropertyChangedCallback(MaxWidthProperty, ChangeMaxWidth);
            RegisterPropertyChangedCallback(MarginProperty, ChangeMargin);
            RegisterPropertyChangedCallback(ElevationLevelProperty, (a, b) => {
                ModalContent.Translation = new Vector3(0, 0, ElevationLevel);
            });
            RegisterPropertyChangedCallback(DropShadowProperty, (a, b) => {
                if (_shadow != null) _shadow.Color = DropShadow ? Colors.Black : Colors.Transparent;
            });
            RegisterPropertyChangedCallback(TitleProperty, (a, b) => {
                if (TitleText != null) TitleText.Visibility = String.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;
            });
        }

        private void CheckOverrideStyles() {
            foreach (var resource in Application.Current.Resources) {
                if (resource.Value is Style style && style.TargetType == typeof(Modal) && resource.Key.GetType() != typeof(string)) {
                    Style = style;
                    break;
                }
            }
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
            CloseButton = (ButtonBase)GetTemplateChild("CloseButton");

            ElementCompositionPreview.SetIsTranslationEnabled(AnimationBorder, true);

            if (!CanUseThemeShadow) ModalContent.SizeChanged += (a, b) => {
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
                UpdateCornersRadius(CornersRadius);
                Animate(Windows.UI.Composition.AnimationDirection.Normal, animationDuration);
                SetFocus(ModalContent);
            };
            WasShowed = true;
        }

        public event EventHandler<object> Closed;

        public void Show() {
            ModalsManager.Add(this);
            popup.IsOpen = true;
        }

        public async void Hide(object data = null) {
            ModalsManager.Remove(this);
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

        private void ChangeShadowCornersRadius(DependencyObject sender, DependencyProperty dp) {
            double r = (double)GetValue(dp);
            UpdateCornersRadius(r);
        }

        bool dontUpdateMaxWidth = false;
        private void ChangeMaxWidth(DependencyObject sender, DependencyProperty dp) {
            double mw = (double)GetValue(dp);
            if (!dontUpdateMaxWidth) {
                if (ModalContent != null) {
                    ModalContent.MaxWidth = mw;
                    if (!CanUseThemeShadow) ShadowBorder.MaxWidth = mw;
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
                    if (!CanUseThemeShadow) ShadowBorder.Margin = t;
                    dontUpdateMargin = true;
                    Margin = new Thickness(0);
                    dontUpdateMargin = false;
                }
            }
        }

        private void UpdateCornersRadius(double r) {
            if(ModalContent != null && ShadowBorder != null) {
                ModalContent.CornerRadius = new CornerRadius(r);
                if (!CanUseThemeShadow) {
                    ShadowBorder.RadiusX = r;
                    ShadowBorder.RadiusY = r;
                    _shadow.Mask = DropShadow ? ShadowBorder.GetAlphaMask() : null;
                }
            }
        }

        private void SetupShadow() {
            if (CanUseThemeShadow) {
                SetUpThemeShadow();
            } else {
                var compositor = ElementCompositionPreview.GetElementVisual(ModalContent).Compositor;
                _visual = compositor.CreateSpriteVisual();
                _visual.Size = ModalContent.RenderSize.ToVector2();
                _visual.Offset = new Vector3(0, 0, 0);

                _shadow = compositor.CreateDropShadow();
                _shadow.Offset = new Vector3(0, 30, 0);
                _shadow.BlurRadius = 42;
                _shadow.Color = DropShadow ? Colors.Black : Colors.Transparent;
                _shadow.Opacity = 0.1f;
                _shadow.Mask = ShadowBorder.GetAlphaMask();
                _visual.Shadow = _shadow;
                ElementCompositionPreview.SetElementChildVisual(ShadowBorder, _visual);
            }
        }

        bool isNewShadowRendered = false;
        private void SetUpThemeShadow() {
            if (isNewShadowRendered) return;
            isNewShadowRendered = true;
            DialogWrapper.Children.Remove(ShadowBorder);

            ModalContent.Translation = new Vector3(0, 0, ElevationLevel);
            if (ModalContent.Shadow == null) ModalContent.Shadow = new ThemeShadow();
        }

        private void Animate(Windows.UI.Composition.AnimationDirection direction, int duration) {
            if (Window.Current.Bounds.Width > ModalContent.MaxWidth) {
                PerformZoomAnimation(direction, duration);
            } else {
                PerformSlideAnimation(direction, duration);
            }
        }

        private void PerformZoomAnimation(Windows.UI.Composition.AnimationDirection direction, int duration) {
            if (direction == Windows.UI.Composition.AnimationDirection.Reverse) duration = duration / 3 * 2;

            Visual dvisual = ElementCompositionPreview.GetElementVisual(LayoutRoot);
            Compositor dcompositor = dvisual.Compositor;

            var size = Window.Current.Bounds;
            float cx = (float)size.Width / 2;
            float cy = (float)size.Height / 2;

            dvisual.Scale = new Vector3(1.13f, 1.13f, 1);
            dvisual.Opacity = 0;
            dvisual.CenterPoint = new Vector3(cx, cy, 1);

            Vector3KeyFrameAnimation vfa = dcompositor.CreateVector3KeyFrameAnimation();
            vfa.InsertKeyFrame(1f, new Vector3(1, 1, 1));
            vfa.Duration = TimeSpan.FromMilliseconds(duration);
            vfa.Direction = direction;
            vfa.IterationCount = 1;

            ScalarKeyFrameAnimation sdfa = dcompositor.CreateScalarKeyFrameAnimation();
            sdfa.InsertKeyFrame(1, 1);
            sdfa.Duration = TimeSpan.FromMilliseconds(duration);
            sdfa.Direction = direction;
            sdfa.IterationCount = 1;

            dvisual.StartAnimation("Scale", vfa);
            dvisual.StartAnimation("Opacity", sdfa);
        }

        private void PerformSlideAnimation(Windows.UI.Composition.AnimationDirection direction, int duration) {
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
