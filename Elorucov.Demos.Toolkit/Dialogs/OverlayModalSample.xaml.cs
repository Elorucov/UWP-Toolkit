using Elorucov.Toolkit.UWP.Controls;
using Windows.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Elorucov.Demos.Toolkit.Dialogs {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverlayModalSample : OverlayModal {
        public OverlayModalSample() {
            this.InitializeComponent();
        }

        private void CloseModal(object sender, RoutedEventArgs e) {
            Hide();
        }
    }
}
