using EasyCom.General;
using EasyCom.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Path = System.IO.Path;
using EasyCom.CustomStr;

namespace EasyCom
{
    public partial class Page_ConnectionTab : Page
    {

        private static string CommandListFileName = "/commands.json";
        private static string CommandListFilePath = Path.Combine(App.DataPath, CommandListFileName);


        public double ToolBarMinWidth;
        public double ToolBarCurrentWidth;

        public PopupDialogHost PopupDialogHostTools { get; set; }
        public PopupDialogHost PopupDialogHostReceive { get; set; }
        public TestViewModel testViewModel = new TestViewModel();
        private ConnectionTabHelper ConnectionTabHelper = null;
        private CustomStrManager customStrManager = null;
        private PageCustomStrBookmarkDialog dialogCustomStrBookmark = null;
        private List<ListBoxItem_CustomStr> CustomStrDisplayList = new List<ListBoxItem_CustomStr>();

        public ChartsExample ChartsPage { get; set; }


        public Page_ConnectionTab(ConnectionTabHelper connectionTabHelper)
        {
            InitializeComponent();

            this.ConnectionTabHelper = connectionTabHelper;
            customStrManager = ((MainWindow)App.Current.MainWindow).CustomStrManager;
            dialogCustomStrBookmark = new PageCustomStrBookmarkDialog(customStrManager.CustomStrTabList);
            dialogCustomStrBookmark.PopupDialog.OnClose = (s, arg) =>
            {
                CustomStrTabRefresh();
                ListBox_CustomStr_Refresh();
            };

            
            this.ToolBarMinWidth = this.ToolBarColumn.MinWidth;
            //CommandListFilePath = ;
            Debug.WriteLine(CommandListFilePath);
            PopupDialogHostTools = new PopupDialogHost(this.DialogHostFrame, this.ToolBarMain);
            PopupDialogHostReceive = new PopupDialogHost(this.DialogHostFrameReceive,this.ReceiveMain);


            WidgetInitial();
            this.Combo_CustomStrTab.SelectionChanged += Combo_MultiCommand_Table_SelectionChanged;
            Debug.WriteLine("Init");
            TextBox_Receive.DataContext = testViewModel;

            customStrManager.OnLoadFinish = CustomStrLoadFinish;
            customStrManager.LoadCustomStrAsync();
            this.Buttom_CustomStrBookMark.Click += Buttom_CustomStrBookMark_Click;
            this.Charts.Content = ChartsPage = new ChartsExample();
            this.SizeChanged += Page_ConnectionTab_SizeChanged;
        }

        private void Page_ConnectionTab_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ToolBarColumn.MaxWidth = this.ActualWidth * 0.7;
        }

        private void Buttom_CustomStrBookMark_Click(object sender, RoutedEventArgs e)
        {
            PopupDialogHost popupDialogHost = ((MainWindow)App.Current.MainWindow).PopupDialogHost;
            popupDialogHost.Show(dialogCustomStrBookmark.PopupDialog);


        }

        private void Combo_MultiCommand_Table_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox_CustomStr_Refresh();
        }

        private void WidgetInitial()
        {
            this.Button_Command_Add.Click += Button_Command_Add_Click;
            this.Combo_CustomStrTab.DisplayMemberPath = "Name";
            //this.ListBox_Command.Data
        }

        private void ListBox_CustomStr_Refresh()
        {
            CustomStrTab selectedItem = (CustomStrTab)Combo_CustomStrTab.SelectedItem;
            if (selectedItem is null)
                return;
            List<CustomStrData> SelectedList = selectedItem.StrList;
            int newCount = SelectedList.Count;
            int oldCount = CustomStrDisplayList.Count;
            if (newCount > oldCount)
            {
                for (int i = 0; i < newCount - oldCount; i++)
                {
                    CustomStrDisplayList.Add(new ListBoxItem_CustomStr(null,ConnectionTabHelper));
                }
            }
            else if (newCount < oldCount)
            {
                for (int i = 0; i < oldCount - newCount; i++)
                {
                    CustomStrDisplayList.RemoveAt(oldCount - 1-i);
                }
            }
            for (int index=0;index<newCount;index++)
            {
                CustomStrDisplayList.ElementAt(index).Data = SelectedList.ElementAt(index);

            }
            this.ListBox_CustomStr.ItemsSource=null;
            this.ListBox_CustomStr.ItemsSource = CustomStrDisplayList;
            
        }

        public void CustomStrTabRefresh()
        {
            CustomStrTab selectedItem = (CustomStrTab)Combo_CustomStrTab.SelectedItem;
            
            this.Combo_CustomStrTab.ItemsSource = null;
            this.Combo_CustomStrTab.ItemsSource = customStrManager.CustomStrTabList;

            if (customStrManager.CustomStrTabList.IndexOf(selectedItem) != -1)
                this.Combo_CustomStrTab.SelectedItem = selectedItem;
            else
                this.Combo_CustomStrTab.SelectedIndex = 0;
        }


        private void Receive_Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            if (Receive_Text.Document.Blocks.Count > 0)
            {
                Paragraph paragraph = (Paragraph)Receive_Text.Document.Blocks.ElementAt(0);
                if (paragraph.Inlines.Count > 0)
                {
                    //Run dd = (Run)paragraph.Inlines.First();
                    //Debug.WriteLine(dd.Text.Length);

                }
            }
            */
        }

        private void Button_ToolBarVisible_Click(object sender, RoutedEventArgs e)
        {
            if (this.TabControl_Tools.Visibility == Visibility.Visible)
            {
                ToolBarCurrentWidth = this.ToolBarColumn.ActualWidth;
                ((MaterialDesignThemes.Wpf.PackIcon)((Border)this.Button_ToolBarVisible.Content).Child).Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowLeftBold;
                this.GridSpiltter_Tools.IsEnabled = false;
                this.TabControl_Tools.Visibility = Visibility.Collapsed;
                this.ToolBarColumn.Width = new GridLength(this.GridSpiltter_Tools.Width + 2 * this.GridSpiltter_Tools.Margin.Left);
                this.ToolBarColumn.MinWidth = 0;
            }
            else
            {
                this.GridSpiltter_Tools.IsEnabled = true;
                ((MaterialDesignThemes.Wpf.PackIcon)((Border)this.Button_ToolBarVisible.Content).Child).Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowRightBold;
                this.TabControl_Tools.Visibility = Visibility.Visible;
                this.ToolBarColumn.Width = new GridLength(ToolBarCurrentWidth);
                this.ToolBarColumn.MinWidth = ToolBarMinWidth;

            }

        }

        private void Button_Command_Add_Click(object sender, RoutedEventArgs e)
        {
            PageCutstomCommandDialog customCommandDialog = new PageCutstomCommandDialog();
            customCommandDialog.Data = new CustomStrData("", "", "");

            customCommandDialog.PopupDialog.OnConfirm=(s,arg) => {
                ((CustomStrTab)Combo_CustomStrTab.SelectedItem).StrList.Add(customCommandDialog.Data);
                ListBox_CustomStr_Refresh();
            };

            PopupDialogHostTools.Show(customCommandDialog.PopupDialog);
            

        }

        private void CustomStrLoadFinish()
        {
            this.Dispatcher.Invoke(()=> {
                CustomStrTabRefresh();
                if (Combo_CustomStrTab.HasItems)
                    Combo_CustomStrTab.SelectedIndex = 0;
            });
        }

        

        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyname = null)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
                }
            }
        }
        public class TestViewModel : ViewModelBase
        {
            private ReceiveModel receiveModel;
            public TestViewModel()
            {
                receiveModel = new ReceiveModel();
            }
            public string text
            {
                get { return receiveModel.text; }
                set { receiveModel.text = value; OnPropertyChanged(); }
            }

        }
        public class ReceiveModel
        {
            public string text { get; set; } = "";
            public ReceiveModel()
            {

            }
        }
    }
}
