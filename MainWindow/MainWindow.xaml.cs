
using EasyCom.Connection;
using EasyCom.General;
using EasyCom.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using static EasyCom.Settings.MainWindowOption;

namespace EasyCom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private ConnectionTabHelper connectionTabHelper;

        private PopupDialogHost popupDialogHost = null;

        private readonly Updater updater;

        private MainWindowOption mainWindowOption;
        internal MainWindowOption Options { get => mainWindowOption; set => mainWindowOption = value; }

        public ConnectionTabHelper ConnectionTabHelper { get => connectionTabHelper; set => connectionTabHelper = value; }
        public PopupDialogHost PopupDialogHost { get => popupDialogHost; set => popupDialogHost = value; }


        private readonly ControlCommandHandle controlCommandHandle;
        
        public CustomStr.CustomStrManager CustomStrManager { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            CustomStrManager = new CustomStr.CustomStrManager(this);
            PopupDialogHost = new PopupDialogHost(this.Popup_DialogFrame, this.Main);
            ConnectionTabHelper = new ConnectionTabHelper(this);
            Options = new MainWindowOption(this);

            ConnectionTypeListInit();

            this.WindowHeader.MouseLeftButtonDown += WindowHeader_MouseLeftButtonDown;
            SettingWidgetsInit();
            
            
            updater = new Updater(this);
            if (((App)Application.Current).UpdateFinish)
            {
                PageUpdateFinishDialog updateFinish = new PageUpdateFinishDialog();
                updateFinish.Label_Statue.Content = "更新完成";
                updateFinish.Button_Cancel.Visibility = Visibility.Hidden;
                updateFinish.Label_StatueDescription.Content = String.Format(CultureInfo.InvariantCulture,"已更新至 {0}", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
                
                PopupDialogHost.CurrentDialog = new PopupDialog(updateFinish);
                PopupDialogHost.Show();
            }
            else
            {
                updater.CheckNewVersion();
            }
            
            controlCommandHandle = new ControlCommandHandle(this);
            if (((App)Application.Current).ShowConsole)
            {
                ConsoleThread console = new ConsoleThread(this);
                console.CommandHandle = controlCommandHandle;
                console.Start();
                
            }
            this.Loaded += (s, e) => { controlCommandHandle.StartUpCommandHandle(); };
            Application.Current.Exit += App_Exit;


            ProcessHandle processHandle = new ProcessHandle();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            CustomStrManager.SaveToFile();
        }

        private void SettingWidgetsInit()
        {
            Combo_Receive_LineEnding.ItemsSource = Options.LineEndingTypeList;
            
            Combo_Send_LineEnding.ItemsSource = Options.LineEndingTypeList;
            Combo_Receive_DecodeType.ItemsSource = Options.EncodingTypeList;
            Combo_Send_Encoding.ItemsSource = Options.EncodingTypeList;

            Toggle_Receive_AutoSpilt.Checked += Toggle_Receive_AutoSpilt_CheckedChange;
            Toggle_Receive_AutoSpilt.Unchecked += Toggle_Receive_AutoSpilt_CheckedChange;
            Toggle_Receive_ShowTime.Checked += Toggle_Receive_ShowTime_CheckedChange; ;
            Toggle_Receive_ShowTime.Unchecked += Toggle_Receive_ShowTime_CheckedChange;
            Combo_Receive_LineEnding.SelectionChanged += Combo_Receive_LineEnding_SelectionChanged;
            TextBox_Receive_Timeout.TextChanged += TextBox_Receive_Timeout_TextChanged;
            Combo_Receive_DecodeType.SelectionChanged += Combo_Receive_DecodeType_SelectionChanged;

            Combo_Send_LineEnding.SelectionChanged += Combo_Send_LineEnding_SelectionChanged;
            Combo_Send_Encoding.SelectionChanged += Combo_Send_Encoding_SelectionChanged;
            //Toggle_Send_Hex.Checked += Toggle_Send_Hex_CheckedChange;
            //Toggle_Send_Hex.Unchecked += Toggle_Send_Hex_CheckedChange;
            Toggle_Send_ShowOnReceive.Checked += Toggle_Send_ShowOnReceive_CheckedChange;
            Toggle_Send_ShowOnReceive.Unchecked += Toggle_Send_ShowOnReceive_CheckedChange;

            //CheckBox_AutoSender_Enable.Checked += CheckBox_AutoSender_Enable_CheckedChange;
            //CheckBox_AutoSender_Enable.Unchecked += CheckBox_AutoSender_Enable_CheckedChange;
            TextBox_AutoSender_Interval.TextChanged += TextBox_AutoSender_Interval_TextChanged;

            //CheckBox_AutoSender_AmountEnable.Checked += CheckBox_AutoSender_AmountEnable_CheckedChange; ;
            //CheckBox_AutoSender_AmountEnable.Unchecked += CheckBox_AutoSender_AmountEnable_CheckedChange;
            TextBox_AutoSender_Amount.TextChanged += TextBox_AutoSender_Amount_TextChanged;
            Button_AutoSender.Click += Button_AutoSender_Click;


            TextBox_Send_Text.TextChanged += TextBox_Send_Text_TextChanged;
            TextBox_Send_FilePath.TextChanged += TextBox_Send_Path_TextChanged;
            
            Button_ReceiveWindow_Clear.Click += Button_ReceiveWindow_Clear_Click;
            Button_ReceiveWindow_Freeze.Click += Button_ReceiveWindow_Freeze_Click;
            Button_ReceiveWindow_PrintNewLine.Click += Button_ReceiveWindow_PrintNewLine_Click;

            Button_Update.Click += Button_Update_Click;
            Panel_UpdateNotify.Visibility = Visibility.Hidden;

            Button_About.Click += Button_About_Click;
            Button_Connection_Confirm.Click += Button_Connection_Confirm_Click;
            Button_Connection_Cancel.Click += Button_Connection_Cancel_Click;

            Button_Send_OpenFile.Click += Button_OpenFile_Click;
            Button_Send_File.Click += Button_Send_File_Click;
            TextBox_Send_FileSize.TextChanged += TextBox_Send_FileSize_TextChanged;
            TextBox_Send_FileInterval.TextChanged += TextBox_Send_FileInterval_TextChanged;
        }

        private void Button_AutoSender_Click(object sender, RoutedEventArgs e)
        {
            ToolBarSetting setting = connectionTabHelper.CurrentTabData.ToolBarSetting;
            bool enable = setting.SendAutoSenderEnable;
            if (enable)
            {
                connectionTabHelper.CurrentTabData.AutoSendRobot.Stop();
            }
            else
            {
                connectionTabHelper.CurrentTabData.AutoSendRobot.Start();
            }
            setting.SendAutoSenderEnable = !enable;
            UpdateAutoSenderInfo(null,null);
        }

        public void UpdateAutoSenderInfo(object sender, object e)
        {
            ToolBarSetting setting = connectionTabHelper.CurrentTabData.ToolBarSetting;
            string targetAmouns = setting.SendAutoSenderAmount.ToString();
            if (setting.SendAutoSenderAmount == 0)
            {
                targetAmouns = "無限";
            }
            Label_AutoSender_INFO.Content = String.Format(CultureInfo.InvariantCulture, " {0} / {1}", setting.SendAutoSenderCurrentAmount, targetAmouns);
            if (setting.SendAutoSenderEnable)
            {
                this.Button_AutoSender.Content = "X";
                this.TextBox_AutoSender_Amount.IsEnabled = false;
                this.TextBox_AutoSender_Interval.IsEnabled = false;

            }
            else
            {
                this.Button_AutoSender.Content = "GO";
                this.TextBox_AutoSender_Amount.IsEnabled = true;
                this.TextBox_AutoSender_Interval.IsEnabled = true;
            }
            

        }

        private void Combo_Send_Encoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            connectionTabHelper.CurrentTabData.ToolBarSetting.SendEncodingType = Combo_Send_Encoding.SelectedItem as EncodingItem;

        }

        private void TextBox_Send_FileInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool success = int.TryParse(TextBox_Send_FileInterval.Text, out int result);
            if (success && result >= 0)
            {
                connectionTabHelper.CurrentTabData.ToolBarSetting.SendFileInterval = result;
            }
            else
            {
                TextBox_Send_FileInterval.Text = "0";
            }
        }

        private void TextBox_Send_FileSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool success = int.TryParse(TextBox_Send_FileSize.Text,out int result);
            if (success && result > 0)
            {
                connectionTabHelper.CurrentTabData.ToolBarSetting.SendFileBufferSize = result;
            }
            else
            {
                TextBox_Send_FileSize.Text = "256";
            }
        }



        private void Button_Send_File_Click(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.SendFile();
        }

        private void Button_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.DefaultExt = ".png";
            //dlg.Filter = "";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                //string filename = dlg.FileName;
                TextBox_Send_FilePath.Text = dlg.FileName;
            }
        }

        private void Button_Connection_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((IPageSetting)connectionTabHelper.CurrentTabData.ConnectionType.AdvanceSettingsPage).SettingsRestore(connectionTabHelper.CurrentTabData.ToolBarSetting.ConnectionSettings);
            SettingChangedCallBack(false);
        }

        private void Button_Connection_Confirm_Click(object sender, RoutedEventArgs e)
        {
            bool successful = connectionTabHelper.CurrentTabData.ApplySetting();
            SettingChangedCallBack(!successful);
        }

        private void Button_About_Click(object sender, RoutedEventArgs e)
        {
            PopupDialogHost.CurrentDialog = new PageAboutDialog().PopupDialog;
            PopupDialogHost.Show();
        }

        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            updater.startUpdateCheck();
        }

        public void Button_ReceiveWindow_PrintNewLine_Click(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveWindowText.Append("\n");
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveWindowTextUpdated = true;
        }

        public void Button_ReceiveWindow_Freeze_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveWindowFreeze)
            {
                ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveWindowFreeze = false;
                this.Button_ReceiveWindow_Freeze_Content.Content = "凍結";
                this.Button_ReceiveWindow_Freeze_ICON.Kind = MaterialDesignThemes.Wpf.PackIconKind.Snowflake;
            }
            else
            {
                ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveWindowFreeze = true;
                this.Button_ReceiveWindow_Freeze_Content.Content = "解凍";
                this.Button_ReceiveWindow_Freeze_ICON.Kind = MaterialDesignThemes.Wpf.PackIconKind.SnowflakeMelt;
            }
        }

        public void Button_ReceiveWindow_Clear_Click(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveWindowText.Clear();
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveWindowTextUpdated = true;
        }

        public void ConnectionTypeListInit()
        {
            this.ComboBox_Connection_Type.ItemsSource = Options.ConnectionTypes;
            this.ComboBox_Connection_Type.SelectionChanged += Connection_Type_SelectionChanged;
            
        }

        private void Button_Connection_Connect_Click(object sender, RoutedEventArgs e)
        {

            if (ConnectionTabHelper.CurrentTabData.Connected)
            {
                ConnectionTabHelper.CurrentTabData.Disconnect();
            }
            else
            {
                bool a = ConnectionTabHelper.CurrentTabData.SaveSettingFromAdvancedSettingPage();
                if (a)
                    ConnectionTabHelper.CurrentTabData.Connect();
                else
                    Console.WriteLine("\nFail:");
            }
            
        }
        public bool Button_Connection_Connect_Available
        {
            set
            {
                if (value == true)
                {
                    Button_Connection_Connect.Tag = true;
                    Button_Connection_Connect.Content = "中斷";

                }
                else
                {
                    Button_Connection_Connect.Tag = false;
                    Button_Connection_Connect.Content = "連線";
                    Button_Connection_Confirm.IsEnabled = false;
                    Button_Connection_Cancel.IsEnabled = false;
                }

            }
            get
            {
                return (bool)Button_Connection_Connect.Tag;
            }
        }

        public void Text_Send_Click(object sender, RoutedEventArgs e)
        {
            
            ConnectionTabHelper.CurrentTabData.SendData(this.TextBox_Send_Text.Text,DateTime.Now);
        }
        private void File_Send_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Connection_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedIndex = ComboBox_Connection_Type.SelectedIndex;
            
            ConnectionTabHelper.CurrentTabData?.ConnectionTypeChoose(SelectedIndex);
            if (SelectedIndex != -1)
            {
                ConnectionType connectionType = Options.ConnectionTypes.ElementAt(SelectedIndex);
                this.Frame_Connection_Setting.Content = connectionType.AdvanceSettingsPage;
                if (connectionType.AdvanceSettingsPage != null)
                {
                    ((IPageSetting)connectionType.AdvanceSettingsPage).BeSelected();
                }
                else
                {
                    this.Frame_Connection_Setting.Content = new Page_Setting_Uncreate();
                    this.Button_Connection_Connect.IsEnabled = false;
                }
            }
            else
            {
                this.Frame_Connection_Setting.Content = null;
                this.Button_Connection_Connect.IsEnabled = false;
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_WinMax_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                Left = 100;
                Top = 100;
            }

            else
                WindowState = WindowState.Normal;

        }

        private void Button_WinMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void WindowHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                var location = Mouse.GetPosition(this);
                double percentX = location.X / this.ActualWidth;
                Debug.WriteLine(location.X + " DD " + location.Y);

                WindowState = WindowState.Normal;
                this.Left = location.X - this.ActualWidth * percentX;
                this.Top = location.Y - this.WindowHeader.ActualHeight / 2;
            }

            this.DragMove();
        }

        //Bottom Receive Tab

        private void Toggle_Receive_ShowTime_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Toggle_Receive_AutoSpilt_CheckedChange(object sender, RoutedEventArgs e)
        {
            bool Result = ((ToggleButton)sender).IsChecked.Value;
            Dock_Receive_AutoSpilt_Advance.IsEnabled = Result;
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveAutoSpilt = Result;
            //MessageBox.Show("IsChecked:" + ((ToggleButton)sender).IsChecked);
        }

        private void Toggle_Send_ShowOnReceive_CheckedChange(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendShowOnReceive = ((ToggleButton)sender).IsChecked.Value;
        }

        private void CheckBox_AutoSender_AmountEnable_CheckedChange(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendAutoSenderAmountEnable = ((CheckBox)sender).IsChecked.Value;
        }

        private void TextBox_AutoSender_Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool result = uint.TryParse(((TextBox)sender).Text, out uint val);
            if (result)
            {
                ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendAutoSenderAmount = val;
            }
            else
                ((TextBox)sender).Text = "0";

            UpdateAutoSenderInfo(null,null);
        }

        private void CheckBox_AutoSender_Enable_CheckedChange(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendAutoSenderEnable = ((CheckBox)sender).IsChecked.Value;
        }

        private void TextBox_AutoSender_Interval_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool result = uint.TryParse(((TextBox)sender).Text, out uint val);
            if (result)
            {
                ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendAutoSenderInterval = val;
            }
            else
                ((TextBox)sender).Text = "100";


        }

        private void Toggle_Send_Hex_CheckedChange(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendHex = ((ToggleButton)sender).IsChecked.Value;
        }                                        
                                                 
        private void Combo_Send_LineEnding_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendLineEnding = (LineEndingItem)((ComboBox)sender).SelectedItem;
        }                                        

        private void TextBox_Receive_Timeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveTimeOut = uint.Parse(((TextBox)sender).Text);
        }

        private void Combo_Receive_LineEnding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveLineEnding = (LineEndingItem)((ComboBox)sender).SelectedItem;
        }
        private void Toggle_Receive_ShowTime_CheckedChange(object sender, RoutedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveShowTime = ((ToggleButton)sender).IsChecked.Value;
        }

        private void TextBox_Send_Path_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendPath = ((TextBox)sender).Text;
            this.Button_Send_File.IsEnabled = File.Exists(this.TextBox_Send_FilePath.Text);
        }

        private void TextBox_Send_Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.SendText = ((TextBox)sender).Text;
        }

        private void Combo_Receive_DecodeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConnectionTabHelper.CurrentTabData.ToolBarSetting.ReceiveEncodingType = (EncodingItem)((ComboBox)sender).SelectedItem;
        }

        //If Setting has already be changed in AdvancedSettingPage , this function will be called
        public void SettingChangedCallBack(bool isChanged)
        {
            //change icon status
            if (connectionTabHelper.CurrentTabData!= null&&connectionTabHelper.CurrentTabData.Connected && isChanged)
            {
                Button_Connection_Confirm.IsEnabled = true;
                Button_Connection_Cancel.IsEnabled = true;
            }
            else
            {
                Button_Connection_Confirm.IsEnabled = false;
                Button_Connection_Cancel.IsEnabled = false;
            }
        }
    }
}

