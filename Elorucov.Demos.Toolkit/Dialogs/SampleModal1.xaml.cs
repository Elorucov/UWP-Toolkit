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
using Windows.UI.Xaml.Navigation;
using Elorucov.Toolkit.UWP.Controls;
using Windows.UI;
using Elorucov.Demos.Toolkit.Helpers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.UI.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using System.Collections.ObjectModel;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Elorucov.Demos.Toolkit.Dialogs {
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SampleModal1 : Modal {
        public SampleModal1() {
            this.InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e) {
            Hide();
        }

        int i = 0;

        private void Add(object sender, RoutedEventArgs e) {
            i++;
            sp.Children.Insert(0, new TextBlock { Text = $"TextBlock {i}" });
        }

        private void Bkg(object sender, RoutedEventArgs e) {
            MaxWidth = 320;
        }

        private void ChangeCornerRadius(object sender, RoutedEventArgs e) {
            CornerRadius += 8;
            Margin = new Thickness(8);
        }

        private void ChangeShadow(object sender, RoutedEventArgs e) {
            DropShadow = !DropShadow;
        }

        private void ChangeFSD(object sender, RoutedEventArgs e) {
            FullSizeDesired = !FullSizeDesired;
        }

        private void Load(object sender, RoutedEventArgs e) {
           
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
