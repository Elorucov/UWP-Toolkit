using System;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Elorucov.Toolkit.UWP.Controls {
    public class OverlayModal : ContentControl, IModal {
        Popup popup;
        bool WasShowed = false;

        ContentPresenter OverlayModalRoot;

        public OverlayModal() {
            this.DefaultStyleKey = typeof(OverlayModal);

            popup = new Popup();
            popup.Child = this;
        }

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();
            OverlayModalRoot = (ContentPresenter)GetTemplateChild(nameof(OverlayModalRoot));

            Resize();
            Window.Current.SizeChanged += (a, b) => Resize(b.Size);
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += (a, b) => Resize();
            WasShowed = true;
        }

        public event EventHandler<object> Closed;

        public void Show() {
            ModalsManager.Add(this);
            popup.IsOpen = true;
        }

        public void Hide(object data = null) {
            ModalsManager.Remove(this);
            Closed?.Invoke(this, data);
            popup.IsOpen = false;
        }

        #region Internal

        private void Resize() => Resize(new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height));

        private void Resize(Size size) {
            popup.Width = size.Width;
            popup.Height = size.Height;
            Width = size.Width;
            Height = size.Height;
        }

        #endregion
    }
}