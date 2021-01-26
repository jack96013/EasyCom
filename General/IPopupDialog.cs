using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCom.General
{
    public interface IPopupDialog
    {
        PopupDialog PopupDialog { get;}
        void OnLoaded();
        void OnClose();
    }
}
