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
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasyCom.CustomStr;
using EasyCom.General;

namespace EasyCom.Widget
{
    /// <summary>
    /// Dialog_CutstomCommand.xaml 的互動邏輯
    /// </summary>
    public partial class PageCutstomCommandDialog : Page, IPopupDialog
    {
        public PopupDialog PopupDialog { get; }
        private CustomStrData _Data = null;
        public PageCutstomCommandDialog()
        {
            InitializeComponent();
            PopupDialog = new PopupDialog(this);
            this.Button_Cancel.Click += Button_Cancel_Click;
            this.Button_Confirm.Click += Button_Confirm_Click;
            this.TextBox_Name.TextChanged += EmptyCheck;
            EmptyCheck(null,null);
        }

        private void EmptyCheck(object sender, TextChangedEventArgs e)
        {
            if (this.TextBox_Name.Text == "")
                this.Button_Confirm.IsEnabled = false;
            else
                this.Button_Confirm.IsEnabled = true;
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            Data.Name = TextBox_Name.Text;
            Data.Description = TextBox_Description.Text;
            Data.Text = TextBox_Command.Text;
            PopupDialog.OnConfirmInvoke();
            PopupDialog.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            PopupDialog.Close();
        }

        public CustomStrData Data 
        {
            get
            {
                return _Data;
            }

            set
            {
                _Data = value;
            }
        }

        void IPopupDialog.OnLoaded()
        {
            TextBox_Name.Text= Data.Name;
            TextBox_Description.Text= Data.Description;
            TextBox_Command.Text=Data.Text;
        }

        void IPopupDialog.OnClose()
        {

        }
    }
}
