using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Elorucov.Toolkit.UWP.Controls {
    public sealed class Avatar : ButtonBase {

        #region Properties

        public static readonly DependencyProperty ImageProperty =
        DependencyProperty.Register(nameof(Image), typeof(Uri), typeof(Avatar), new PropertyMetadata(default(Uri)));

        public Uri Image {
            get { return (Uri)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty =
        DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(Avatar), new PropertyMetadata(default(string)));

        public string DisplayName {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public int Size { get { return (int)Math.Min(ActualWidth, ActualHeight); } }

        #endregion

        #region Private fields

        Grid AvatarContainer;
        Border BackgroundBorder;
        TextBlock AvatarInitials;
        Image AvatarImage;
        BitmapImage AvatarImageSource;

        #endregion

        public Avatar() {
            this.DefaultStyleKey = typeof(Avatar);
            RegisterPropertyChangedCallback(ImageProperty, (a, b) => { SetImage(); });
            RegisterPropertyChangedCallback(DisplayNameProperty, (a, b) => { SetInitials(); });
            RegisterPropertyChangedCallback(BackgroundProperty, (a, b) => { SetBackground(); });
            SizeChanged += (a, b) => ChangeDecodeSize();
        }

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();
            AvatarContainer = (Grid)GetTemplateChild(nameof(AvatarContainer));
            BackgroundBorder = (Border)GetTemplateChild(nameof(BackgroundBorder));
            AvatarImage = (Image)GetTemplateChild(nameof(AvatarImage));
            AvatarInitials = (TextBlock)GetTemplateChild(nameof(AvatarInitials));
            SetBackground();
            SetInitials();
            SetImage();
        }

        #region Private methods

        private void SetBackground() {
            if (Background != null) {
                BackgroundBorder.Background = Background;
            } else {
                string i = (String.IsNullOrEmpty(DisplayName)) ? "" : DisplayName;
                BackgroundBorder.Background = GetColor(RecursiveDivide(i.GetHashCode(), 2, 10));
            }
        }

        private void SetInitials() {
            if (AvatarInitials == null) return;
            if (!String.IsNullOrEmpty(DisplayName)) {
                string result = "";
                string[] split = DisplayName.Split(' ');
                for (int i = 0; i < Math.Min(2, split.Length); i++) {
                    result += split[i][0];
                }
                AvatarInitials.Text = result.ToUpper();
            } else {
                AvatarInitials.Text = "";
            }
            SetBackground();
        }

        private void SetImage() {
            if (AvatarImage == null) return;
            if (Image != null) {
                AvatarImage.Visibility = Visibility.Visible;
                AvatarImageSource = new BitmapImage {
                    UriSource = Image, DecodePixelType = DecodePixelType.Logical,
                };
                ChangeDecodeSize();
                AvatarImage.Source = AvatarImageSource;
                BackgroundBorder.Visibility = Visibility.Collapsed;
            } else {
                AvatarImage.Visibility = Visibility.Collapsed;
                BackgroundBorder.Visibility = Visibility.Visible;
            }
        }

        private void ChangeDecodeSize() {
            if (Size > 0 && AvatarImageSource != null) {
                AvatarImageSource.DecodePixelHeight = Size;
                AvatarImageSource.DecodePixelWidth = Size;
            }
        }

        private int RecursiveDivide(int num, int divideto, int max) {
            do {
                num = num / divideto;
            } while (num > max);
            return num;
        }

        private SolidColorBrush GetColor(int index) {
            switch (index) {
                case 0: return new SolidColorBrush(Color.FromArgb(255, 0, 128, 128));
                case 1: return new SolidColorBrush(Color.FromArgb(255, 240, 74, 72));
                case 2: return new SolidColorBrush(Color.FromArgb(255, 255, 162, 30));
                case 3: return new SolidColorBrush(Color.FromArgb(255, 248, 202, 64));
                case 4: return new SolidColorBrush(Color.FromArgb(255, 95, 191, 100));
                case 5: return new SolidColorBrush(Color.FromArgb(255, 89, 169, 235));
                case 6: return new SolidColorBrush(Color.FromArgb(255, 101, 128, 240));
                case 7: return new SolidColorBrush(Color.FromArgb(255, 200, 88, 220));
                case 8: return new SolidColorBrush(Color.FromArgb(255, 250, 80, 65));
                case 9: return new SolidColorBrush(Color.FromArgb(255, 159, 36, 179));
                case 10: return new SolidColorBrush(Color.FromArgb(255, 0, 172, 193));
                default: return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            }
        }

        #endregion
    }
}
