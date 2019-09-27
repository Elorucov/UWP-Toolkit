using Elorucov.Toolkit.UWP.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Elorucov.Demos.Toolkit {
    public class MenuItem {
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri PreviewImage { get; set; }
        public double RotateAngle { get; set; } = 0;
        public Type Page { get; set; }
    }
        
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Menu : Page {
        List<Grid> menuCards = new List<Grid>();
        private static MenuItem SelectedMenuItem;

        ObservableCollection<MenuItem> MenuItems = new ObservableCollection<MenuItem> {
            new MenuItem {
                Title = "User avatars",
                Description = "Description",
                PreviewImage = new Uri("ms-appx:///Assets/Previews/UserAvatars.png"),
                Page = typeof(Pages.UserAvatars)
            },
            new MenuItem {
                Title = "Media slider",
                Description = "for audioplayers",
                PreviewImage = new Uri("ms-appx:///Assets/Previews/MediaSlider.png"),
                Page = typeof(Pages.MediaSliderSample)
            },
            new MenuItem {
                Title = "Modals",
                Description = "Simple modal",
                PreviewImage = new Uri("ms-appx:///Assets/Previews/Modals.png"),
                Page = typeof(Pages.Modals)
            },
            //new MenuItem {
            //    Title = "Hint flyout",
            //    Description = "Description",
            //    PreviewImage = new Uri("https://sun1-23.userapi.com/c849432/v849432217/18ad60/Vls0Q3sb1UY.jpg"),
            //    Page = typeof(Pages.Modals)
            //},
        };

        public Menu() {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            Loaded += Load;
        }

        private void Load(object sender, RoutedEventArgs e) {
            SystemNavigationManager.GetForCurrentView().BackRequested += (a, b) => {
                if(!ModalsManager.HaveOpenedModals) {
                    if (Frame.CanGoBack) {
                        b.Handled = true;
                        Frame.GoBack(new DrillInNavigationTransitionInfo());
                    }
                }
            };
            Frame.Navigated += (a, b) => {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Frame.BackStackDepth > 0 ?
                AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            };

            MainMenu.SizeChanged += (a, b) => ResizeCards(b.NewSize.Width);
            MainMenu.ItemsSource = MenuItems;
        }

        private void InitResizeEvent(object sender, RoutedEventArgs e) {
            Grid g = sender as Grid;
            menuCards.Add(g);
            ResizeCards(MainMenu.ActualWidth);
        }

        private void ResizeCards(double b) {
            foreach (Grid g in menuCards) {
                double s = (b) / 240;
                g.Width = (int)s > 1 ? (b / (int)s) - 5 : b;
            }
        }

        private async void PrepareBackAnimation(object sender, RoutedEventArgs e) {
            if(SelectedMenuItem != null) {
                MainMenu.ScrollIntoView(SelectedMenuItem, ScrollIntoViewAlignment.Default);
                MainMenu.UpdateLayout();
                ConnectedAnimation ca = ConnectedAnimationService.GetForCurrentView().GetAnimation("connectback");
                if (ca != null) {
                    bool res = await MainMenu.TryStartConnectedAnimationAsync(ca, SelectedMenuItem, "ItemTitle");
                    System.Diagnostics.Debug.WriteLine($"Result: {res}");
                }
            }
        }

        private void OpenPage(object sender, ItemClickEventArgs e) {
            SelectedMenuItem = e.ClickedItem as MenuItem;
            ConnectedAnimation ca = MainMenu.PrepareConnectedAnimation("connect", e.ClickedItem, "ItemTitle");
            Frame.Navigate(SelectedMenuItem.Page, null);
        }
    }
}
