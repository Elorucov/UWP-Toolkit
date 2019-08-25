using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Elorucov.Toolkit.UWP.Controls {
    public sealed class UserAvatars : Control {

        #region Properties

        public static readonly DependencyProperty AvatarsProperty =
                DependencyProperty.Register("Avatars", typeof(ObservableCollection<BitmapImage>), typeof(UserAvatars), new PropertyMetadata(default(ObservableCollection<BitmapImage>)));

        public ObservableCollection<BitmapImage> Avatars {
            get { return (ObservableCollection<BitmapImage>)GetValue(AvatarsProperty); }
            set { SetValue(AvatarsProperty, value); }
        }

        public static readonly DependencyProperty MaxDisplayedAvatarsProperty =
               DependencyProperty.Register("MaxDisplayedAvatars", typeof(int), typeof(UserAvatars), new PropertyMetadata(default(int)));

        public int MaxDisplayedAvatars {
            get { return (int)GetValue(MaxDisplayedAvatarsProperty); }
            set { SetValue(MaxDisplayedAvatarsProperty, value); }
        }

        public static readonly DependencyProperty OverrideAvatarsCountProperty =
               DependencyProperty.Register("OverrideAvatarsCount", typeof(int), typeof(UserAvatars), new PropertyMetadata(0));

        public int OverrideAvatarsCount {
            get { return (int)GetValue(OverrideAvatarsCountProperty); }
            set { SetValue(OverrideAvatarsCountProperty, value); }
        }

        #endregion

        #region Private fields

        StackPanel Root;

        #endregion

        public UserAvatars() {
            this.DefaultStyleKey = typeof(UserAvatars);
            RegisterPropertyChangedCallback(AvatarsProperty, (a, b) => { RenderAvatars(); });
            RegisterPropertyChangedCallback(MaxDisplayedAvatarsProperty, (a, b) => { RenderAvatars(); });
            RegisterPropertyChangedCallback(OverrideAvatarsCountProperty, (a, b) => { RenderAvatars(); });
            Debug.WriteLine("UserAvatars: init.");
        }

        bool alreadyLoaded = false;
        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();
            Root = (StackPanel)GetTemplateChild("Root");
            Debug.WriteLine("UserAvatars: Template applied.");
            if (!alreadyLoaded) { RenderAvatars(); alreadyLoaded = true; }
        }

        int el = 0;

        private void RenderAvatars() {
            Debug.WriteLine("UserAvatars: RenderAvatars().");
            if (OverrideAvatarsCount > 0 && OverrideAvatarsCount <= MaxDisplayedAvatars)
                throw new ArgumentException("OverrideAvatarsCount value can not be less than MaxDisplayedAvatars value.");
            int max = MaxDisplayedAvatars;
            var avatars = Avatars;
            if(Root != null && avatars != null && avatars.Count > 0 && max >= 0) {
                Debug.WriteLine("UserAvatars: Rendering!");
                Root.Children.Clear();
                el = 0;
                for (el = 0; el < Math.Min(max, avatars.Count); el++) {
                    Root.Children.Add(GetBittenCircle(avatars[el]));
                }
                if(OverrideAvatarsCount > 0) {
                    Root.Children.Add(GetCircle());
                } else {
                    if (avatars.Count > max) {
                        Root.Children.Add(GetCircle());
                    } else {
                        Root.Children.Remove(Root.Children.Last());
                        Root.Children.Add(GetCircle(avatars.Last()));
                    }
                }
            }
        }

        private Viewbox GetBittenCircle(BitmapImage avatar) {
            avatar.DecodePixelType = DecodePixelType.Logical;

            Viewbox vb = new Viewbox();
            vb.Stretch = Stretch.Uniform;

            Canvas c = new Canvas();
            if (el > 0) c.Margin = new Thickness(-8,0,0,0);
            c.Width = 48;
            c.Height = 48;

            ImageBrush ib = new ImageBrush();
            ib.ImageSource = avatar;
            ib.Stretch = Stretch.UniformToFill;

            string pathxaml = $"<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Fill='#A0A0A0' StrokeThickness='0' Data='M 24 2 A 22 22 0 0 0 2 24 A 22 22 0 0 0 24 46 A 22 22 0 0 0 42.847656 35.296875 A 24.000001 24 0 0 1 40 24 A 24 24 0 0 1 42.84375 12.689453 A 22 22 0 0 0 24 2 z ' />";
            Path p = XamlReader.Load(pathxaml) as Path;
            p.Fill = ib;
            //p.Fill = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));

            c.Children.Add(p);
            vb.Child = c;
            return vb;
        }

        private Viewbox GetCircle(BitmapImage avatar = null) {
            Viewbox vb = new Viewbox();
            vb.Stretch = Stretch.Uniform;
            vb.RequestedTheme = ElementTheme.Dark;

            Grid g = new Grid();
            if (el > 0) g.Margin = new Thickness(-8, 0, 0, 0);
            g.Width = 48;
            g.Height = 48;

            Ellipse e = new Ellipse();
            e.Fill = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"];
            if (avatar != null) {
                avatar.DecodePixelType = DecodePixelType.Logical;
                e.Fill = new ImageBrush() { ImageSource = avatar };
            }
            e.Width = 44;
            e.Height = 44;
            g.Children.Add(e);

            if(avatar == null) {
                int z = OverrideAvatarsCount > 0 ? OverrideAvatarsCount : Avatars.Count;
                TextBlock t = new TextBlock();
                t.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                t.LineHeight = 16;
                t.VerticalAlignment = VerticalAlignment.Center;
                t.TextAlignment = TextAlignment.Center;
                t.Text = $"+{z - MaxDisplayedAvatars}";
                g.Children.Add(t);
            }

            vb.Child = g;
            return vb;
        }
    }
}
