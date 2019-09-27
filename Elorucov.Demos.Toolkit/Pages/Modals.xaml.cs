using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Elorucov.Demos.Toolkit.Pages {
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Modals : Page {
        public Modals() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            ConnectedAnimation ca = ConnectedAnimationService.GetForCurrentView().GetAnimation("connect");
            if (ca != null) ca.TryStart(PageTitle);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("connectback", PageTitle);
        }

        private void btn01(object sender, RoutedEventArgs e) {
            var m = new Dialogs.SampleModal1();
            m.Show();
        }

        private void btn02(object sender, RoutedEventArgs e) {
            var m = new Dialogs.SampleModal2();
            m.Show();
        }

        //private void btn03(object sender, RoutedEventArgs e) {
        //    var hm = new Dialogs.SampleHalfModal1();
        //    hm.Show();
        //}
    }
}
