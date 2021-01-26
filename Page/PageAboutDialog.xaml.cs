using EasyCom.General;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyCom
{
    /// <summary>
    /// About.xaml 的互動邏輯
    /// </summary>
    public partial class PageAboutDialog : Page,IPopupDialog
    {
        public PopupDialog PopupDialog { get; }

        public PageAboutDialog()
        {
            InitializeComponent();
            PopupDialog = new PopupDialog(this);
            this.Label_Version.Content = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            this.Button_Confirm.Click += Button_Confirm_Click;
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            PopupDialog.OnConfirmInvoke();
            PopupDialog.Close();
        }


        void IPopupDialog.OnLoaded()
        {
        }

        void IPopupDialog.OnClose()
        {
            
        }
    }
}
