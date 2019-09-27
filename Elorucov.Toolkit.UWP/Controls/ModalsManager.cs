using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Elorucov.Toolkit.UWP.Controls {
    public class ModalsManager {
        private static List<IModal> _openedModals = new List<IModal>();
        private static bool isEventRegistered = false;

        internal static void Add(IModal modal) {
            _openedModals.Add(modal);
            if(!isEventRegistered) {
                SystemNavigationManager.GetForCurrentView().BackRequested += ModalsManager_BackRequested;
            }
        }

        internal static void Remove(IModal modal) {
            _openedModals.Remove(modal);
        }

        private static void ModalsManager_BackRequested(object sender, BackRequestedEventArgs e) {
            if (_openedModals.Count == 0) return;
            e.Handled = true;
            _openedModals.Last().Hide();
        }

        public static bool HaveOpenedModals { get { return _openedModals.Count > 0; } }
    }
}
