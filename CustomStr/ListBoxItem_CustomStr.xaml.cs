using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasyCom.CustomStr;


namespace EasyCom
{
    /// <summary>
    /// ListBoxItem_Command.xaml 的互動邏輯
    /// </summary>
    public partial class ListBoxItem_CustomStr : ListBoxItem
    {
        private CustomStrData data = null;
        public CustomStrData Data {
            get { return data; }
            set { data = value; Refresh(); }
        
        }
        private ConnectionTabHelper connectionTabHelper;
        public ListBoxItem_CustomStr(CustomStrData customStrData, ConnectionTabHelper connectionTabHelper)
        {
            InitializeComponent();
            this.Data = customStrData;
            this.connectionTabHelper = connectionTabHelper;
            if (customStrData != null)
                Refresh();

            this.TextBox_Command.TextChanged += TextBox_Command_TextChanged;
        }

        private void TextBox_Command_TextChanged(object sender, TextChangedEventArgs e)
        {
            Data.Text = TextBox_Command.Text;
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            connectionTabHelper.CurrentTabData.SendData(this.Data.Text, DateTime.Now);
        }

        public void Refresh()
        {
            if (Data != null)
            {
                this.TextBox_Command.Text = Data.Text;
                if (Data.Description != null)
                    this.Grid_Main.ToolTip = Data.Description;
                this.Button_Send.Content = Data.Name;
            }
        }
        
    }
}
