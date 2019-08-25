using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Elorucov.Toolkit.UWP.Helpers;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Hosting;
using System.Diagnostics;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.System.Profile;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Elorucov.Toolkit.UWP.Controls {
    public enum HalfModalState {
        Closed, Half, Full
    }

    public class HalfModal : ContentControl, IModal {

        #region Private fields

        Popup popup;
        bool WasShowed = false;

        Grid LayoutRoot;
        Grid ITRoot;
        Border Layer;
        StackPanel ModalContent;
        Grid TitleBar;
        Button CloseButton;

        TextBlock DebugText1;
        TextBlock DebugText2;
        TextBlock DebugText3;

        public float StartHeight { get; set; } = 0;

        Compositor _compositor;
        Visual _root;
        VisualInteractionSource _is;
        InteractionTracker _tracker;
        InteractionTrackerOwner _itowner;
        Vector3 oldPosition;
        float Half { get {
                float f = _tracker.MinPosition.Y + StartHeight;
                return f > 0 ? 0 : f;
            } }
        byte scrollMode = 0;

        #endregion

        #region Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HalfModal), new PropertyMetadata(default(string)));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleBarBackgroundProperty =
            DependencyProperty.Register("TitleBarBackground", typeof(Brush), typeof(HalfModal), new PropertyMetadata(default(Brush)));

        public Brush TitleBarBackground {
            get { return (Brush)GetValue(TitleBarBackgroundProperty); }
            set { SetValue(TitleBarBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarForegroundProperty =
            DependencyProperty.Register("TitleBarForeground", typeof(Brush), typeof(HalfModal), new PropertyMetadata(default(Brush)));

        public Brush TitleBarForeground {
            get { return (Brush)GetValue(TitleBarForegroundProperty); }
            set { SetValue(TitleBarForegroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarVisibilityProperty =
            DependencyProperty.Register("TitleBarVisibility", typeof(Visibility), typeof(HalfModal), new PropertyMetadata(default(Visibility)));

        public Visibility TitleBarVisibility {
            get { return (Visibility)GetValue(TitleBarVisibilityProperty); }
            set { SetValue(TitleBarVisibilityProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(HalfModalState), typeof(HalfModal), new PropertyMetadata(default(HalfModalState)));

        public HalfModalState State {
            get { return (HalfModalState)GetValue(StateProperty); }
            private set { SetValue(StateProperty, value); }
        }

        #endregion

        public HalfModal() {
            this.DefaultStyleKey = typeof(HalfModal);
            popup = new Popup();
            popup.Child = this;
        }

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();

            LayoutRoot = (Grid)GetTemplateChild("LayoutRoot");
            ITRoot = (Grid)GetTemplateChild("ITRoot");
            Layer = (Border)GetTemplateChild("Layer");
            ModalContent = (StackPanel)GetTemplateChild("ModalContent");
            TitleBar = (Grid)GetTemplateChild("TitleBar");
            CloseButton = (Button)GetTemplateChild("CloseButton");

            DebugText1 = (TextBlock)GetTemplateChild("DebugText1");
            DebugText2 = (TextBlock)GetTemplateChild("DebugText2");
            DebugText3 = (TextBlock)GetTemplateChild("DebugText3");

            Resize();
            Window.Current.SizeChanged += (a, b) => { Resize(b.Size); };
            Loaded += (a, b) => {
                Resize(); // resize popup and layoutroot
                SetupModal();
                _tracker.TryUpdatePosition(new Vector3(0, _tracker.MinPosition.Y + 1, 0));
                OpenHalf();
            };
            ITRoot.PointerPressed += (a, b) => {
                try {
                    _is.TryRedirectForManipulation(b.GetCurrentPoint(this));
                } catch (Exception ex) {
                    Debug.WriteLine("TryRedirectForManipulation: " + ex.ToString());
                }
            };
            ITRoot.PointerWheelChanged += (a, b) => {
                var o = b.GetCurrentPoint(a as UIElement);
                int z = o.Properties.MouseWheelDelta;
                float d = (float)z * 1.5f;
                DebugText3.Text = $"Wheel: {z}\nDelta: {d}";
                if (d != 0 && _tracker.Position.Y >= _tracker.MinPosition.Y + StartHeight && _tracker.Position.Y <= _tracker.MaxPosition.Y) {
                    if (_tracker.Position.Y == _tracker.MaxPosition.Y && d < 0) return;
                    _tracker.TryUpdatePositionWithAnimation(GetScrollKFA(new Vector3(0, _tracker.Position.Y - d, 0)));
                }
            };
            CloseButton.Click += (a, b) => {
                Hide();
            };
            WasShowed = true;
        }

        public event EventHandler<object> Closed;

        public void Show() {
            popup.IsOpen = true;
        }

        public void Hide(object data = null) {
            Close(data);
        }

        #region Internal functions and events

        private void Resize() => Resize(new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height));

        private void Resize(Size size) {
            popup.Width = size.Width;
            popup.Height = size.Height;
            Width = size.Width;
            Height = size.Height;

            ModalContent.MinHeight = Window.Current.Bounds.Height;
            UpdateInteractionTrackerValues();
            if (_tracker != null) {
                switch (State) {
                    case HalfModalState.Full: if (_tracker.Position.Y < 0) OpenFull(); break;
                    case HalfModalState.Half: OpenHalf(); break;
                }
            }
        }

        private void SetupModal() {
            // Fix display orientation
            DisplayInformation.AutoRotationPreferences = DisplayInformation.GetForCurrentView().CurrentOrientation;

            // Set up visual interaction source
            _root = ElementCompositionPreview.GetElementVisual(ITRoot);
            _compositor = _root.Compositor;
            _is = VisualInteractionSource.Create(_root);
            _is.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;

            // Set up interaction tracker owner
            _itowner = new InteractionTrackerOwner();
            _itowner.StateOrValuesChanged += (r, s) => {
                DebugText1.Text = $"State: {s.Item1}\nPos:   {s.Item2.Y}";
                ChangeScroll(s);
                oldPosition = s.Item2;
            };

            // Set up interaction tracker
            _tracker = InteractionTracker.CreateWithOwner(_compositor, _itowner);
            _tracker.InteractionSources.Add(_is);

            UpdateInteractionTrackerValues();

            var exp = _compositor.CreateExpressionAnimation("-tracker.Position");
            exp.SetReferenceParameter("tracker", _tracker);

            var bv = ElementCompositionPreview.GetElementVisual(ModalContent);
            bv.StartAnimation("Offset", exp);

            ExpressionAnimation _titleAnimation = _compositor.CreateExpressionAnimation("Clamp(tracker.Position.Y, 0, tracker.MaxPosition.Y)");
            _titleAnimation.SetReferenceParameter("tracker", _tracker);

            var tv = ElementCompositionPreview.GetElementVisual(TitleBar);
            tv.StartAnimation("Offset.Y", _titleAnimation);
        }

        private void UpdateInteractionTrackerValues() {
            if (_tracker != null) {
                float f = (float)ModalContent.ActualHeight - (float)Window.Current.Bounds.Height;
                _tracker.MinPosition = new Vector3(0, -(float)Window.Current.Bounds.Height, 0);
                _tracker.MaxPosition = new Vector3(0, f < 0 ? 0 : f, 0);
                DebugText3.Text = $"F: {f}; H: {Window.Current.Bounds.Height}";
            }
        }

        private void ChangeScroll(Tuple<InteractionTrackerState, Vector3> s) {
            float diff = s.Item2.Y - oldPosition.Y;
            string st = $"Diff: {Math.Round(diff, 1)}";

            if (diff != 0) {
                scrollMode = diff > 0 ? (byte)1 : (byte)2;
            }

            st += $"\nMode: {scrollMode}";

            if (s.Item2.Y < Half) {
                if (s.Item1 == InteractionTrackerState.Idle) {
                    if (scrollMode == 1 && _tracker.Position.Y != Half) OpenHalf();
                    if (scrollMode == 2 && _tracker.Position.Y != _tracker.MinPosition.Y) Close();
                }
            }
            if (s.Item2.Y > Half && s.Item2.Y < 0) {
                if (s.Item1 == InteractionTrackerState.Idle) {
                    if (scrollMode == 1 && _tracker.Position.Y != 0) OpenFull();
                    if (scrollMode == 2 && _tracker.Position.Y != Half) OpenHalf();
                }
            }
            if(s.Item2.Y <= _tracker.MinPosition.Y && s.Item1 != InteractionTrackerState.Interacting) {
                // Unfix display orientation and hide popup
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
                popup.IsOpen = false;
            }
            DebugText2.Text = st;
        }

        private CompositionAnimation GetScrollKFA(Vector3 newPosition) {
            Vector3KeyFrameAnimation kfa = _compositor.CreateVector3KeyFrameAnimation();
            kfa.Duration = TimeSpan.FromMilliseconds(400);
            kfa.InsertKeyFrame(1, newPosition);
            return kfa;
        }

        #endregion

        #region Change state

        private void OpenHalf() {
            _tracker.TryUpdatePositionWithAnimation(GetScrollKFA(new Vector3(0, Half, 0)));
            State = HalfModalState.Half;
        }

        private void OpenFull() {
            _tracker.TryUpdatePositionWithAnimation(GetScrollKFA(new Vector3(0, 0, 0)));
            State = HalfModalState.Full;
        }

        private void Close(object data = null) {
            _tracker.TryUpdatePositionWithAnimation(GetScrollKFA(new Vector3(0, _tracker.MinPosition.Y, 0)));
            State = HalfModalState.Closed;
            Closed?.Invoke(this, data);
        }

        #endregion
    }
}
