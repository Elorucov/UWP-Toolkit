using Elorucov.Toolkit.UWP.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace Elorucov.Demos.Toolkit
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(360, 480));
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void btn01(object sender, RoutedEventArgs e) {
            var m = new Dialogs.SampleModal1();
            m.Padding = new Thickness(0);
            m.Show();
        }

        private void btn02(object sender, RoutedEventArgs e) {
            var m = new Dialogs.SampleModal2();
            m.Show();
        }

        private void btn03(object sender, RoutedEventArgs e) {
            var hm = new Dialogs.SampleHalfModal1();
            hm.Title = "Half-modal!";
            hm.Show();
        }

        private void LoadUserAvatars(object sender, RoutedEventArgs e) {
            string el = "https://pp.userapi.com/c847021/v847021629/205e9e/n8E9p-bmhAM.jpg";
            string eee = "https://sun1-28.userapi.com/c849432/v849432217/18ad60/Vls0Q3sb1UY.jpg";
            string grs = "https://sun1-87.userapi.com/c840236/v840236023/829e7/skMZSSfugD8.jpg";
            string sdl = "https://pp.userapi.com/c850536/v850536781/1460ff/N-UOCTmgnVk.jpg";
            string mrh = "https://sun1-20.userapi.com/c858228/v858228758/1452c/vLPD8kTfAWs.jpg";
            string nik = "https://sun1-26.userapi.com/c850128/v850128006/8633f/yRgM9VtYjBA.jpg";
            string mp3 = "https://sun1-27.userapi.com/c840335/v840335671/849ea/-4rawXT-t0g.jpg";
            string vas = "https://sun1-20.userapi.com/c855424/v855424547/73a0b/B60DX8PtOJc.jpg";
            string tsi = "https://sun1-22.userapi.com/c830708/v830708352/1c50b3/W-ZDnTalKLE.jpg";

            List<string> s = new List<string> { el, eee, grs, sdl, mrh, nik, mp3, vas, tsi };
            s = Shuffle(s);

            ObservableCollection<BitmapImage> avatars = new ObservableCollection<BitmapImage>();
            foreach(string st in s) {
                avatars.Add(new BitmapImage(new Uri(st)));
            }
            avas.Avatars = avatars;
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
            if(ka) {
                avas.OverrideAvatarsCount = i;
                avasinfo.Text = $"H: {avas.Height}\nCount: {avas.Avatars.Count}\nMax displayed: {avas.MaxDisplayedAvatars}\nOverrideAvatarsCount: {avas.OverrideAvatarsCount}";
            }
        }

        private static Random rng = new Random();

        private List<string> Shuffle(List<string> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
