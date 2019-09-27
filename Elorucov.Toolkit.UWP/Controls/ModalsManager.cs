using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.WriteLine($"ModalsManager: Opened {modal.GetHashCode()}.");
            if (!isEventRegistered) {
                SystemNavigationManager.GetForCurrentView().BackRequested += ModalsManager_BackRequested;
                isEventRegistered = true;
            }
        }

        internal static void Remove(IModal modal) {
            _openedModals.Remove(modal);
            Debug.WriteLine($"ModalsManager: Closed {modal.GetHashCode()}.");
        }

        private static void ModalsManager_BackRequested(object sender, BackRequestedEventArgs e) {
            if (_openedModals.Count == 0) return;
            e.Handled = true;
            _openedModals.Last().Hide();
        }

        public static bool HaveOpenedModals { get { return _openedModals.Count > 0; } }
    }
}
