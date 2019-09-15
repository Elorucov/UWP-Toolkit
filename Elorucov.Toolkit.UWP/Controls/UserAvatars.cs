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
    public class UserAvatarItem {
        public string Name { get; set; }
        public string Initials { get { return GetInitials(); } }
        public BitmapImage Image { get; set; }

        private string GetInitials() {
            string str = String.Empty;
            if (!String.IsNullOrEmpty(Name)) {
                string[] s = Name.Split(' ');
                for(int i = 0; i < 2; i++) {
                    str += s[i][0];
                }
            }
            return str.ToUpperInvariant();
        }
    }

    public sealed class UserAvatars : Control {

        #region Properties

        public static readonly DependencyProperty AvatarsProperty =
                DependencyProperty.Register("Avatars", typeof(ObservableCollection<UserAvatarItem>), typeof(UserAvatars), new PropertyMetadata(default(ObservableCollection<BitmapImage>)));

        public ObservableCollection<UserAvatarItem> Avatars {
            get { return (ObservableCollection<UserAvatarItem>)GetValue(AvatarsProperty); }
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
            RegisterPropertyChangedCallback(HeightProperty, (a, b) => { RenderAvatars(); });
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
                Root.RequestedTheme = ElementTheme.Dark;
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

        private Viewbox GetBittenCircle(UserAvatarItem avatar) {
            if(avatar.Image != null) {
                avatar.Image.DecodePixelType = DecodePixelType.Logical;
                avatar.Image.DecodePixelHeight = (int)Height;
            }

            Viewbox vb = new Viewbox();
            vb.Stretch = Stretch.Uniform;

            Canvas c = new Canvas();
            if (el > 0) c.Margin = new Thickness(-8,0,0,0);
            c.Width = 48;
            c.Height = 48;

            Brush b = new ImageBrush();
            if (avatar.Image != null) {
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = avatar.Image;
                ib.Stretch = Stretch.UniformToFill;
                b = ib;
            } else {
                b = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"];
                Border tb = new Border();
                tb.Height = 48;
                tb.Width = 48;
                if (!String.IsNullOrEmpty(avatar.Initials)) {
                    tb.Child = GetTextBlockForCircle(avatar.Initials, true);
                    c.Children.Add(tb);
                }
            }

            string pathxaml = $"<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Fill='#A0A0A0' StrokeThickness='0' Data='M 24 2 A 22 22 0 0 0 2 24 A 22 22 0 0 0 24 46 A 22 22 0 0 0 42.847656 35.296875 A 24.000001 24 0 0 1 40 24 A 24 24 0 0 1 42.84375 12.689453 A 22 22 0 0 0 24 2 z ' />";
            Path p = XamlReader.Load(pathxaml) as Path;
            p.Fill = b;

            if(!String.IsNullOrEmpty(avatar.Name)) ToolTipService.SetToolTip(c, avatar.Name);
            c.Children.Insert(0, p);
            vb.Child = c;
            return vb;
        }

        private Viewbox GetCircle(UserAvatarItem avatar = null) {
            Viewbox vb = new Viewbox();
            vb.Stretch = Stretch.Uniform;
            vb.RequestedTheme = ElementTheme.Dark;

            Grid g = new Grid();
            if (el > 0) g.Margin = new Thickness(-8, 0, 0, 0);
            g.Width = 48;
            g.Height = 48;

            Ellipse e = new Ellipse();
            e.Fill = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"];
            if(avatar != null) {
                if (avatar.Image != null) {
                    avatar.Image.DecodePixelType = DecodePixelType.Logical;
                    avatar.Image.DecodePixelHeight = (int)Height;
                    e.Fill = new ImageBrush() { ImageSource = avatar.Image };
                } else if(avatar.Image == null && !String.IsNullOrEmpty(avatar.Initials)) {
                    g.Children.Add(GetTextBlockForCircle(avatar.Initials, true));
                }
            }
            e.Width = 44;
            e.Height = 44;
            g.Children.Insert(0, e);
            if (avatar != null && !String.IsNullOrEmpty(avatar.Name)) ToolTipService.SetToolTip(e, avatar.Name);

            if (avatar == null) {
                int z = OverrideAvatarsCount > 0 ? OverrideAvatarsCount : Avatars.Count;
                g.Children.Add(GetTextBlockForCircle($"+{z - MaxDisplayedAvatars}"));
            }

            vb.Child = g;
            return vb;
        }

        private TextBlock GetTextBlockForCircle(string text, bool isBold = false) {
            TextBlock t = new TextBlock();
            t.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            t.LineHeight = 16;
            t.VerticalAlignment = VerticalAlignment.Center;
            t.TextAlignment = TextAlignment.Center;
            if (isBold) t.FontWeight = new Windows.UI.Text.FontWeight() { Weight = 600 };
            t.Text = text;
            return t;
        }
    }
}
