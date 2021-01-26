using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace EasyCom
{
    /// <summary>
    /// Page_ReceiveWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Page_ReceiveWindow : Page
    {
        private MainWindow parentWindow;
        public ICSharpCode.AvalonEdit.TextEditor TextBox_TT;
        public MainWindow ParentWindow
        {
            get { return parentWindow; }
            set { parentWindow = value; }
        }

        public Page_ReceiveWindow()
        {
            InitializeComponent();
            TextBox_TT = this.TextBox_Test;
        }

        public void Clear_Received_Data()
        {
            Receive_Text.Document.Blocks.Clear();
            Receive_Timestamp.Clear();
        }

        private void Receive_Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            Paragraph paragraph = (Paragraph) Receive_Text.Document.Blocks.FirstBlock;
            
            Debug.WriteLine(paragraph.Inlines.Count);
            
        }
    }
}
