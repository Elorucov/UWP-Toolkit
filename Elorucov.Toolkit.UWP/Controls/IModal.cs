using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elorucov.Toolkit.UWP.Controls {
    public interface IModal {
        event EventHandler<object> Closed;
        void Show();
        void Hide(object data = null);
    }
}
