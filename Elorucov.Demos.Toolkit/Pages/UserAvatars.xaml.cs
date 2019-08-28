using Elorucov.Toolkit.UWP.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Elorucov.Demos.Toolkit.Pages {
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class UserAvatars : Page {
        public UserAvatars() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            ConnectedAnimation ca = ConnectedAnimationService.GetForCurrentView().GetAnimation("connect");
            if (ca != null) ca.TryStart(Root);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("connectback", Root);
        }

        private void LoadUserAvatars(object sender, RoutedEventArgs e) {
            ObservableCollection<UserAvatarItem> uai = new ObservableCollection<UserAvatarItem> {
                new UserAvatarItem {
                    Name = "Elchin Orujov",
                    Image = new BitmapImage(new Uri("https://pp.userapi.com/c847021/v847021629/205e9e/n8E9p-bmhAM.jpg")),
                },
                new UserAvatarItem {
                    Name = "Eugene Kotlyarov",
                    Image = new BitmapImage(new Uri("https://sun1-28.userapi.com/c849432/v849432217/18ad60/Vls0Q3sb1UY.jpg")),
                },
                new UserAvatarItem {
                    Name = "Gregory Klyushnikov",
                    Image = new BitmapImage(new Uri("https://sun1-87.userapi.com/c840236/v840236023/829e7/skMZSSfugD8.jpg")),
                },
                new UserAvatarItem {
                    Name = "Egor Metlitsky",
                    Image = new BitmapImage(new Uri("https://sun1-27.userapi.com/c840335/v840335671/849ea/-4rawXT-t0g.jpg")),
                },
                new UserAvatarItem {
                    Name = "Sergey Dilong",
                    Image = new BitmapImage(new Uri("https://pp.userapi.com/c850536/v850536781/1460ff/N-UOCTmgnVk.jpg")),
                },
                new UserAvatarItem {
                    Name = "Nikolay Borisov",
                    Image = new BitmapImage(new Uri("https://sun1-26.userapi.com/c850128/v850128006/8633f/yRgM9VtYjBA.jpg")),
                },
                new UserAvatarItem {
                    Name = "Oleg Zuev (no-avatar test)"
                },
                new UserAvatarItem {
                    Name = "Willy Willy (no-avatar test)"
                },
            };

            uai = Shuffle(uai);
            avas.Avatars = uai;
            avasinfo.Text = $"H: {avas.Height}\nCount: {avas.Avatars.Count}\nMax displayed: {avas.MaxDisplayedAvatars}\nOverrideAvatarsCount: {avas.OverrideAvatarsCount}";
        }

        private void IncreaseMaxDisplayedAvatars(object sender, RoutedEventArgs e) {
            avas.MaxDisplayedAvatars++;
            avasinfo.Text = $"H: {avas.Height}\nCount: {avas.Avatars.Count}\nMax displayed: {avas.MaxDisplayedAvatars}\nOverrideAvatarsCount: {avas.OverrideAvatarsCount}";
        }

        private void DecreaseMaxDisplayedAvatars(object sender, RoutedEventArgs e) {
            avas.MaxDisplayedAvatars--;
            avasinfo.Text = $"H: {avas.Height}\nCount: {avas.Avatars.Count}\nMax displayed: {avas.MaxDisplayedAvatars}\nOverrideAvatarsCount: {avas.OverrideAvatarsCount}";
        }

        private void IncreaseHeight(object sender, RoutedEventArgs e) {
            avas.Height = avas.Height + 4;
            avasinfo.Text = $"H: {avas.Height}\nCount: {avas.Avatars.Count}\nMax displayed: {avas.MaxDisplayedAvatars}\nOverrideAvatarsCount: {avas.OverrideAvatarsCount}";
        }

        private void DecreaseHeight(object sender, RoutedEventArgs e) {
            avas.Height = avas.Height - 4;
            avasinfo.Text = $"H: {avas.Height}\nCount: {avas.Avatars.Count}\nMax displayed: {avas.MaxDisplayedAvatars}\nOverrideAvatarsCount: {avas.OverrideAvatarsCount}";
        }

        private void OverrideAvCntChanged(TextBox sender, TextBoxTextChangingEventArgs args) {
            int i = 0;
            bool ka = Int32.TryParse(oac.Text, out i);
            if (ka) {
                avas.OverrideAvatarsCount = i;
                avasinfo.Text = $"H: {avas.Height}\nCount: {avas.Avatars.Count}\nMax displayed: {avas.MaxDisplayedAvatars}\nOverrideAvatarsCount: {avas.OverrideAvatarsCount}";
            }
        }

        private static Random rng = new Random();

        private ObservableCollection<UserAvatarItem> Shuffle(ObservableCollection<UserAvatarItem> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                UserAvatarItem value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
