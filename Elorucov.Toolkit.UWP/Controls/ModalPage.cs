using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Elorucov.Toolkit.UWP.Controls {
    public class ModalPage : Page {
        public IModal ParentModal { get; internal set; }
    }
}
