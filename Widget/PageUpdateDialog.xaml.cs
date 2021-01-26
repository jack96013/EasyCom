using EasyCom.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyCom
{
    /// <summary>
    /// UpdateWindow.xaml 的互動邏輯
    /// </summary>
    public partial class PageUpdateDialog : Page,IPopupDialog
    {
        private Updater.UpdateInfo updateInfo = new Updater.UpdateInfo();

        public PageUpdateDialog()
        {
            InitializeComponent();
            PopupDialog = new PopupDialog(this);
            this.Button_Confirm.Click += Button_Confirm_Click;
            this.Button_Cancel.Click += Button_Cancel_Click;
            
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            PopupDialog.OnCancelInvoke();
            PopupDialog.Close();
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            PopupDialog.OnConfirmInvoke();
            PopupDialog.Close();
        }

        internal Updater.UpdateInfo UpdateInfo { get => updateInfo; set => updateInfo = value; }
        public PopupDialog PopupDialog { get; }

        public void OnLoaded()
        {
            this.Label_VersionCurrent.Content = UpdateInfo.CurrentVersion;
            this.Label_VersionNew.Content = UpdateInfo.FileVersion;
            if (UpdateInfo.ReleaseDate != "")
                this.Label_VersionNew.Content += string.Format(" ({0})", UpdateInfo.ReleaseDate);
            this.TextBox_ChangeLog.Text = UpdateInfo.ChangeLog;
        }

        public void OnClose()
        {

        }
    }
}
