using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Elorucov.Toolkit.UWP.Controls {
    public sealed class Avatar : ButtonBase {

        #region Properties

        public static readonly DependencyProperty ImageUriProperty =
        DependencyProperty.Register(nameof(ImageUri), typeof(Uri), typeof(Avatar), new PropertyMetadata(default(Uri)));

        public Uri ImageUri {
            get { return (Uri)GetValue(ImageUriProperty); }
            set { SetValue(ImageUriProperty, value); }
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
        Ellipse AvatarImageFallback;
        private static List<Uri> IgnoredLinks { get; } = new List<Uri>();

        #endregion

        public Avatar() {
            this.DefaultStyleKey = typeof(Avatar);
            RegisterPropertyChangedCallback(ImageUriProperty, (a, b) => { SetImage(); });
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
            AvatarImageFallback = (Ellipse)GetTemplateChild(nameof(AvatarImageFallback));

            SetBackground();
            SetInitials();
            SetImage();
        }

        public static void AddUriForIgnore(Uri uri) {
            if (IgnoredLinks.Contains(uri)) return;
            IgnoredLinks.Add(uri);
        }

        #region Private methods

        private void SetBackground() {
            if (BackgroundBorder == null) return;
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
                string[] split = DisplayName.Trim().Split(' ');
                for (int i = 0; i < Math.Min(2, split.Length); i++) {
                    if (split[i].Length == 0) continue;
                    result += split[i][0];
                }
                AvatarInitials.Text = result.ToUpper();
            } else {
                AvatarInitials.Text = "";
            }
            SetBackground();
        }

        private void SetImage() {
            if (BackgroundBorder == null || AvatarImage == null) return;
            BackgroundBorder.Visibility = Visibility.Visible;
            if (AvatarImage == null) return;
            if (ImageUri != null && !IgnoredLinks.Contains(ImageUri)) {
                ChangeImageVisibility(Visibility.Visible);
                BitmapImage bi = new BitmapImage {
                    UriSource = ImageUri, DecodePixelType = DecodePixelType.Logical,
                };
                bi.ImageOpened += (a, b) => BackgroundBorder.Visibility = Visibility.Collapsed;
                bi.ImageFailed += (a, b) => BackgroundBorder.Visibility = Visibility.Visible;
                AvatarImageSource = bi;

                ChangeDecodeSize();
                SetImageSource();
            } else {
                ChangeImageVisibility(Visibility.Collapsed);
            }
        }

        private void ChangeImageVisibility(Visibility visibility) {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") {
                AvatarImageFallback.Visibility = visibility;
            } else {
                AvatarImage.Visibility = visibility;
            }
        }

        private void SetImageSource() {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") {
                AvatarImageFallback.Fill = new ImageBrush {
                    Stretch = Stretch.UniformToFill, ImageSource = AvatarImageSource
                };
            } else {
                AvatarImage.Source = AvatarImageSource;
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
