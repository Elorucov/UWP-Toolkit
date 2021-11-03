using Elorucov.Toolkit.UWP.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class AvatarSamples : Page {
        public AvatarSamples() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            ConnectedAnimation ca = ConnectedAnimationService.GetForCurrentView().GetAnimation("connect");
            if (ca != null) ca.TryStart(PageTitle);

            Avatar.AddUriForIgnore(new Uri("https://vk.com/images/deactivated_200.gif"));
            Avatar.AddUriForIgnore(new Uri("https://vk.com/images/camera_200.png"));
            Avatar.AddUriForIgnore(new Uri("https://vk.com/images/community_200.png"));
            Avatar.AddUriForIgnore(new Uri("https://vk.com/images/icons/im_multichat_200.png"));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("connectback", PageTitle);
        }

        private void IncreaseHeight(object sender, RoutedEventArgs e) {
            Ava.Width += 4;
            Ava.Height += 4;
        }

        private void DecreaseHeight(object sender, RoutedEventArgs e) {
            Ava.Width -= 4;
            Ava.Height -= 4;
        }

        private async void AvaClicked(object sender, RoutedEventArgs e) {
            await (new MessageDialog(Ava.DisplayName, "Clicked.")).ShowAsync();
        }

        private void SetImgSrc(object sender, RoutedEventArgs e) {
            if (!Uri.IsWellFormedUriString(AvaImage.Text, UriKind.Absolute)) return;
            BitmapImage image = new BitmapImage(new Uri(AvaImage.Text));
            Ava.ImageSource = image;
        }
    }
}
