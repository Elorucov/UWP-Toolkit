using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Elorucov.Demos.Toolkit.Pages {
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MediaSliderSample : Page {
        public MediaSliderSample() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            ConnectedAnimation ca = ConnectedAnimationService.GetForCurrentView().GetAnimation("connect");
            if (ca != null) ca.TryStart(PageTitle);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            sp.Background = null;
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("connectback", PageTitle);
        }

        private async void StackPanel_Loaded(object sender, RoutedEventArgs e) {
            await Task.Delay(500);
            sp.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (slider2.BufferingProgress > 1) return;
            slider2.BufferingProgress += 0.05;
        }
    }
}
