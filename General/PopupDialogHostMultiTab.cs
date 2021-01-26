using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EasyCom.General
{
    public class PopupDialogHostMultiTab : PopupDialogHost
    {
        private ConnectionTabHelper TabHelper { get; }
        public PopupDialogHostMultiTab(Frame DialogFrame, Panel ContextPanel,ConnectionTabHelper tabHelper) : base(DialogFrame,ContextPanel)
        {
            this.TabHelper = tabHelper;
        }

        public void ShowDialogForTab(ConnectionTabData tab, PopupDialog dialog)
        {
            if (tab is null)
                return;
            tab.toolBarSetting.PopupDialogReceive = dialog;
            dialog.WindowStatus = PopupDialog.Status.Show;
            if (TabHelper.CurrentTabData== tab)
            {
                Show(dialog);
            }
        }
        public void CloseDialogForTab(ConnectionTabData tab, PopupDialog dialog)
        {
            if (tab is null)
                return;
            tab.toolBarSetting.PopupDialogReceive.Close();
            tab.toolBarSetting.PopupDialogReceive = null;
        }
    }
}
