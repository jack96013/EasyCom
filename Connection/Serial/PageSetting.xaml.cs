using EasyCom.General;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;


namespace EasyCom.Connection.Serial
{
    /// <summary>
    /// Connection_Serial.xaml 的互動邏輯
    /// </summary>
    /// 

    public partial class PageSetting : Page, IPageSetting
    {
        private readonly MainWindow mainWindow;

        public SerialHelper CurrentConnectionHelper { get; set; } //當前連線

        object IPageSetting.currentConnection { get { return CurrentConnectionHelper; } set { CurrentConnectionHelper = (SerialHelper)value; } }

        private CustomBaudrate customBaudrate;
        public List<ConnectionTabData> ComPortUsedTabList { get; } = new List<ConnectionTabData>(); //Save the comport be used by connection tabs
        public List<int> ComPortUsedList { get; } = new List<int>(); //Save the comport be used by connection tabs

        public List<ComPortItem> ComPortList { get; set; } = new List<ComPortItem>();

        private static ManualResetEvent ProcessPortList = new ManualResetEvent(true);


        public delegate void SettingsOnChangedHandler();
        public event SettingsOnChangedHandler SettingsOnChange;
        public int ComboBox_Baudrate_lastSelectedIndex;

        public static Options SerialOptions { get; } = new Options();

        public Type settingsStructType
        {
            get {
                return typeof(Settings);   
            }
        }

        public Action<object,bool> SettingChangedCallBack { get; set ; }

        public PageSetting(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            customBaudrate = new CustomBaudrate(this, this.mainWindow.PopupDialogHost);
            InitializeComponent();

            SerialOptions.BaudRateList.Add(customBaudrate);
            ComboBox_Baudrate.ItemsSource = SerialOptions.BaudRateList;

            this.mainWindow.SourceInitialized += new EventHandler(MainWindow_SourceInitialized);
            this.Button_BaudrateEdit.Visibility = Visibility.Collapsed;

            ComboBoxItemInitial();
            CheckBox_DTR.Click += SettingChanged;
            CheckBox_RTS.Click += SettingChanged;
        }


        private void ComboBoxItemInitial()
        {

            this.ComboBox_ComPort.SelectionChanged += SelectComPort;
            this.ComboBox_ComPort.ItemsSource = ComPortList;

            this.ComboBox_Baudrate.SelectionChanged += SelectBaudrate;
            
            this.ComboBox_ComPort.SelectionChanged += SettingChanged;
            this.ComboBox_Baudrate.SelectionChanged += SettingChanged;
            
            this.Combo_DataBits.ItemsSource = SerialOptions.DataBitsList.List;
            this.Combo_DataBits.SelectedIndex = 0;
            this.Combo_DataBits.SelectionChanged += SettingChanged;
            this.Combo_DataBits.DisplayMemberPath = "Display";

            this.Combo_StopBits.ItemsSource = SerialOptions.StopBitsList.List;
            this.Combo_StopBits.SelectedIndex = 1;
            this.Combo_StopBits.SelectionChanged += SettingChanged;
            this.Combo_StopBits.DisplayMemberPath = "Display";
            

            this.Combo_Parity.ItemsSource = SerialOptions.ParityList.List;
            this.Combo_Parity.SelectedIndex = 0;
            this.Combo_Parity.SelectionChanged += SettingChanged;
            this.Combo_Parity.DisplayMemberPath = "Display";

            this.Combo_Handshake.ItemsSource = SerialOptions.HandshakeList.List;
            this.Combo_Handshake.SelectedIndex = 0;
            this.Combo_Handshake.SelectionChanged += SettingChanged;
            this.Combo_Handshake.DisplayMemberPath = "Display";

        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(mainWindow).Handle);
            if (source != null)
            {
                IntPtr windowHandle = source.Handle;
                source.AddHook(HwndHandler);
                SerialDeviceNotification.RegisterDeviceNotification(windowHandle);
            }
        }

        //該連線被選取，初始化
        public void BeSelected()
        {
            ListSerialPort();
        }
        
        public ComPortItem SelectedRemovedItem = null;

        public void ListSerialPort()
        {
            Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("list comport");
                ProcessPortList.Reset();
                string[] SerialPortName = SerialPort.GetPortNames();
                SerialPortName = SerialPortName.Distinct().ToArray();
                //Array.Sort(SerialPortName);
                List < object[] > serialportData = new List<object[]>();

                Regex regex = new Regex(@"COM(\d+)(?=\D?)");
                for (int k = 0; k < SerialPortName.Length; k++)
                {
                    Match cmpResult = regex.Match(SerialPortName[k]);
                    if (cmpResult.Success)
                    {
                        serialportData.Add(new object[] {int.Parse(cmpResult.Groups[1].ToString(),CultureInfo.InvariantCulture), ""});
                    }
                    
                }
                List<ComPortItem> newPortList = new List<ComPortItem>();
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
                //get all port detail
                foreach (ManagementObject port in searcher.Get())
                {
                    // ex: Arduino Uno (COM7)
                    string result = port.GetPropertyValue("Caption").ToString();
                   
                    Match cmpResult = Regex.Match(result, @"(.+?)\(COM(\d+)(?=\D?)");
                    if (cmpResult.Success)
                    {
                        Match idResult = Regex.Match(port.GetPropertyValue("DeviceID").ToString(), @".+\\VID_(\w+)&PID_(\w+)\\.+");
                        if (idResult.Success)
                        {
                            int vid = int.Parse(idResult.Groups[1].Value, NumberStyles.HexNumber,CultureInfo.InvariantCulture);
                            int pid = int.Parse(idResult.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        int ResultID = int.Parse(cmpResult.Groups[2].Value,CultureInfo.InvariantCulture);
                        string ResultDescription = cmpResult.Groups[1].ToString();

                        object[] element = serialportData.Find((x)=> {return (int)x[0] == ResultID;});
                        element[1] = ResultDescription;
                        newPortList.Add(new ComPortItem((int)element[0], (string)element[1]));
                    }
                }
                newPortList.Sort((x, y) => { return x.ComID.CompareTo(y.ComID); });

                foreach (ConnectionTabData tab in ComPortUsedTabList)
                {
                    SerialHelper serial = tab.ConnectionObject as SerialHelper;
                    if (serial != null)
                    {
                        ComPortItem result = newPortList.Find((x) => string.Format("COM{0}", x.ComID) == serial.SerialPort.PortName);
                        if (result != null)
                        {
                            result.Used = true;
                            result.UsedTab = tab;
                        }
                        else
                        {
                            //找不到，已被移除
                            this.Dispatcher.InvokeAsync(() => {
                                Console.WriteLine(tab.TabItem.Title);
                                tab.ConnectionObject.Close();
                                ConnectionTabHelper tabHelper = mainWindow.ConnectionTabHelper;
                                PageDialog dialog = new PageDialog
                                {
                                    InfoTitle = "痾",
                                    InfoContent = String.Format(CultureInfo.InvariantCulture, "[{0}] 斷線惹...", 0),
                                    Tab = tab
                                };

                                tabHelper.ShowDialogOnReceiveWindow(tab, dialog.PopupDialog);
                            });
                        }
                    }
                }
                //Append to list
                this.Dispatcher.InvokeAsync((() => {
                    int newIndex=-1;
                    if (ComboBox_ComPort.SelectedIndex != -1)
                    {
                        int index = newPortList.FindIndex((x) => { return x.ComID == ((ComPortItem)ComboBox_ComPort.SelectedItem).ComID; });
                        if (index != -1)
                        {
                            //找到
                            newIndex = index;
                        }
                        else
                        {
                            //找不到
                            SelectedRemovedItem = (ComPortItem)ComboBox_ComPort.SelectedItem;
                            SelectedRemovedItem.Removed = true;
                            if (SelectedRemovedItem != null)
                                newPortList.Add(SelectedRemovedItem);
                            newIndex = newPortList.Count-1;
                        }
                    }

                    ComPortList = newPortList;

                    ComboBox_ComPort_Refresh();
                    ComboBox_ComPort.SelectedIndex = newIndex;
                }));
                ProcessPortList.Set();
                
            }, cancellationToken: CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        public void RefreshComPort(object sender, RoutedEventArgs e)
        {
            ListSerialPort();
        }


        public void SelectComPort(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_ComPort.SelectedIndex == ComboBox_Baudrate.Items.Count - 2)
            {
                
            }
            CheckSettingsBeFinished();
        }

        public void SelectBaudrate(object sender, SelectionChangedEventArgs e)
        {

            if (ComboBox_Baudrate.SelectedItem == customBaudrate)
            {

                if (customBaudrate.value == 0)
                {
                    ComboBox_Baudrate.SelectedIndex = ComboBox_Baudrate_lastSelectedIndex;
                    customBaudrate.showDialog();
                }
                else
                {
                    this.Button_BaudrateEdit.Visibility = Visibility.Visible;
                }

            }
            else
            {
                this.Button_BaudrateEdit.Visibility = Visibility.Collapsed;
                ComboBox_Baudrate_lastSelectedIndex = ComboBox_Baudrate.SelectedIndex;
            }

            CheckSettingsBeFinished();
        }

        private void Button_BaudrateEdit_Click(object sender, RoutedEventArgs e)
        {
            customBaudrate.showDialog();
        }
        
        public void ComboBox_BaudRate_Refresh()
        {
            ComboBox_Baudrate.ItemsSource = null;
            ComboBox_Baudrate.ItemsSource = SerialOptions.BaudRateList;
        }

        public void ComboBox_ComPort_Refresh()
        {
            ComboBox_ComPort.ItemsSource = null;
            ComboBox_ComPort.ItemsSource = ComPortList;
        }

        public void CheckSettingsBeFinished()
        {
            if (mainWindow != null)
            {
                bool result = true;
                if (this.ComboBox_ComPort.SelectedIndex == -1)
                    result = false;
                if (this.ComboBox_Baudrate.SelectedIndex == -1)
                    result = false;
                if (result)
                {
                    this.mainWindow.Button_Connection_Connect.IsEnabled = true;

                }
                else
                {
                    if (mainWindow.Button_Connection_Connect != null)
                        this.mainWindow.Button_Connection_Connect.IsEnabled = false;
                }
            }
        }

        private void SettingChanged(object sender, RoutedEventArgs e)
        {
            if (mainWindow.ConnectionTabHelper.CurrentTabData is null)
                return;
            Settings settings = (Settings)mainWindow.ConnectionTabHelper.CurrentTabData.ToolBarSetting.ConnectionSettings;
            Settings newSettings = new Settings();
            GetSetting(newSettings);
            if (settings != null)
                SettingChangedCallBack?.Invoke(this, !settings.Equal(newSettings));
        }


        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == SerialDeviceNotification.WmDevicechange)
            {
                switch ((int)wparam)
                {
                    case SerialDeviceNotification.DbtDeviceremovecomplete:
                        //Device Remove
                        ListSerialPort();
                        break;
                    case SerialDeviceNotification.DbtDevicearrival:
                        //Device Insert
                        ListSerialPort();
                        break;
                }
            }
            handled = false;
            return IntPtr.Zero;
        }

        public void SettingsRestore(object settings)
        {
            //Debug.WriteLine("Apply ->" + parentWindow.ConnectionTabHelper.CurrentTabData.TabItem.Title);
            Settings Settings = (Settings)settings;


            if (Array.IndexOf(SerialOptions.DefaultBaudRate, Settings.Baudrate) == -1)
            {
                customBaudrate.value = Settings.Baudrate;
                ComboBox_BaudRate_Refresh();
                this.ComboBox_Baudrate.SelectedIndex = SerialOptions.BaudRateList.Count - 1;

            }
            else
            {
                this.ComboBox_Baudrate.SelectedItem = Settings.Baudrate;
            }

            this.CheckBox_RTS.IsChecked = Settings.RTSEnable;
            this.CheckBox_DTR.IsChecked = Settings.DTREnable;
            this.Combo_DataBits.SelectedItem = Settings.DataBits;
            this.Combo_StopBits.SelectedItem = Settings.StopBits;
            this.Combo_Parity.SelectedItem = Settings.Parity;
            this.Combo_Handshake.SelectedItem = Settings.Handshake;

            Task.Factory.StartNew(()=> {
                ProcessPortList.WaitOne();
                ComPortItem nextSelect = null;
                Debug.WriteLine(Settings.ComPort, "RESTORE ADV");
                if (Settings.ComPort != -1)
                {
                    bool find = false;
                    //尋找Port是否被移除
                    foreach (ComPortItem k in ComboBox_ComPort.Items)
                    {
                        if (k.ComID == Settings.ComPort)
                        {
                            nextSelect = k;
                            find = true;
                            break;
                        }
                    }
                    //Port had been removed
                    if (!find)
                    {
                        ComPortItem old = new ComPortItem(Settings.ComPort, null);
                        old.Removed = true;
                        ComPortList.Add(old);
                        mainWindow.Dispatcher.Invoke(()=> { ComboBox_ComPort_Refresh(); });
                        nextSelect = old;
                    }
                }
                mainWindow.Dispatcher.Invoke(() =>
                {
                    ComboBox_ComPort.SelectedItem = nextSelect;
                });
            }, cancellationToken: CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        public void GetSetting(object settings)
        {
            Settings Settings = (Settings)settings;

            if (ComboBox_ComPort.SelectedItem != null)
            {
                Settings.ComPort = ((ComPortItem)ComboBox_ComPort.SelectedItem).ComID;
            }
            else
            {
                Settings.ComPort = -1 ;
            }
            

            if (this.ComboBox_Baudrate.SelectedItem == customBaudrate && customBaudrate.value != 0)
            {
                Settings.Baudrate = customBaudrate.value;
            }
            else
            {
                if (this.ComboBox_Baudrate.SelectedItem != null)
                    Settings.Baudrate = (uint)this.ComboBox_Baudrate.SelectedItem;
            }

            Settings.RTSEnable = this.CheckBox_RTS.IsChecked.Value;
            Settings.DTREnable = this.CheckBox_DTR.IsChecked.Value;
            Settings.DataBits = (BasicItem<ushort>)this.Combo_DataBits.SelectedItem;
            Settings.StopBits = (BasicItem<StopBits>)this.Combo_StopBits.SelectedItem;
            Settings.Parity = (BasicItem<Parity>)this.Combo_Parity.SelectedItem;
            Settings.Handshake = (BasicItem<Handshake>)this.Combo_Handshake.SelectedItem;
        }

        public void SetSettingDefault(object settings)
        {
            //Debug.WriteLine("Default ->" + parentWindow.ConnectionTabHelper.CurrentTabData.TabItem.Title);
            Settings Settings = (Settings)settings;
            Settings.ComPort = -1;
            Settings.Baudrate = 9600;
            Settings.RTSEnable = false;
            Settings.DTREnable = false;
            Settings.DataBits = (BasicItem<ushort>)Combo_DataBits.Items.GetItemAt(0);
            Settings.StopBits = (BasicItem<StopBits>)Combo_StopBits.Items.GetItemAt(1);
            Settings.Parity = (BasicItem<Parity>)Combo_Parity.Items.GetItemAt(0);
            Settings.Handshake = (BasicItem<Handshake>)Combo_Handshake.Items.GetItemAt(0);
        }

        public void UsePort(int id)
        {
            ComPortUsedList.Append(id);
            ListSerialPort();
        }
        public void UsePort(ConnectionTabData tab)
        {
            ComPortUsedTabList.Add(tab);
        }
        public void ReleasePort(ConnectionTabData tab)
        {
            ComPortUsedTabList.Remove(tab);
        }

        public void UsedPortAliveCheck()
        {
            foreach (ConnectionTabData data in ComPortUsedTabList)
            {
                SerialHelper serial = ((SerialHelper)data.ConnectionObject);
                if (serial.Connected)
                {
                    ComPortItem result = ComPortList.Find(item => (string.Format("COM{0}", item.ComID) == serial.SerialPort.PortName)&item.Removed);
                    if (result != null)
                    {
                        serial.Close();
                    }
                }
            }
        }

        
    }
}
