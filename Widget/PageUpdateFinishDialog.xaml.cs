using EasyCom.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyCom
{
    /// <summary>
    /// Dialog_UpdateFinish.xaml 的互動邏輯
    /// </summary>
    public partial class PageUpdateFinishDialog : Page, IPopupDialog
    {
        public PopupDialog PopupDialog { get; }
        public enum Statue{Fail,Success};
        public Statue currentStatue =Statue.Fail;

        public PageUpdateFinishDialog()
        {
            InitializeComponent();
            this.PopupDialog = new PopupDialog(this);
            this.Button_Confirm.Click += Button_Confirm_Click;
            this.Button_Cancel.Click += Button_Cancel_Click;
            
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            PopupDialog.Close();
        }

        public void OnLoaded()
        {
            if (currentStatue == Statue.Fail)
            {

            }
            else if (currentStatue == Statue.Success)
            {
                
            }
        }

        public void OnClose()
        {

        }
    }

    
}
