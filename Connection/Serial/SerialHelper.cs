using System;
using System.Diagnostics;
using System.Windows;
using System.IO.Ports;
using System.Globalization;
using EasyCom.General;
using MaterialDesignThemes.Wpf;

namespace EasyCom.Connection.Serial
{
    public partial class SerialHelper : IConnection
    {
        //public SerialPortStream Serial;
        public SerialPort SerialPort { get; set; }
        private readonly ConnectionTabData currentTab;

        private EasyCom.Settings.ToolBarSetting toolBarSettings;

        public Stopwatch stopwatch1 = new Stopwatch();


        public bool Connected {
            get
            {
                return SerialPort.IsOpen;
            }
        }

        public SerialHelper(ConnectionTabData connectionTab)
        {
            this.currentTab = connectionTab;
            this.SerialPort = new SerialPort();
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            toolBarSettings = currentTab.ToolBarSetting;
            currentTab.ToolBarSetting.ConnectionSettings = (Settings)toolBarSettings.ConnectionSettings;
            Debug.WriteLine(this.GetHashCode(),"Serial Create");
            //SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        /// <summary>
        /// Apply new settings ,if fail occurred ,the function return false; 
        /// </summary>
        /// <returns></returns>
        public bool ApplySettings()
        {
            try
            {
                Settings ConnectionSettings = (Settings)currentTab.ConnectionSettings;
                string PortName = String.Format(CultureInfo.InvariantCulture, "COM{0}", ConnectionSettings.ComPort);
                if (SerialPort.PortName != PortName)
                    SerialPort.PortName = String.Format(CultureInfo.InvariantCulture, "COM{0}", ConnectionSettings.ComPort);
                SerialPort.BaudRate = Convert.ToInt32(ConnectionSettings.Baudrate);
                SerialPort.RtsEnable = ConnectionSettings.RTSEnable;
                SerialPort.DtrEnable = ConnectionSettings.DTREnable;
                SerialPort.DataBits = ConnectionSettings.DataBits.Value;
                SerialPort.StopBits = ConnectionSettings.StopBits.Value;
                SerialPort.Parity = ConnectionSettings.Parity.Value;
                SerialPort.Handshake = ConnectionSettings.Handshake.Value;

                SerialPort.ReadTimeout = Convert.ToInt32(toolBarSettings.ReceiveTimeOut);
            }
            catch (System.IO.IOException e)
            {
                ApplyNewSettingOnError(e, "無效的連線參數");
                return false;
            }
            catch (InvalidOperationException e)
            {
                ApplyNewSettingOnError(e, "無效的連線參數");
                return false;
            }
            return true;
        }

        private void ApplyNewSettingOnError(Exception e, string helperText)
        {
            ShowDialog("設定失敗",
                        string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", e.Message, helperText),
                        "重試",
                        () => { currentTab.ApplySetting(); },
                        PackIconKind.CloseCircleOutline);
            //currentTab.ApplyOnFail();
        }

        public void Open()
        {
            if (!Connected)
            {
                ApplySettings();
                try
                {
                    SerialPort.Open();
                    //RefreshSettings();
                    currentTab.onConnectSuccessful();
                    PageSetting pageSetting = currentTab.ConnectionType.AdvanceSettingsPage as PageSetting;
                    pageSetting.UsePort(currentTab);
                }
                catch (UnauthorizedAccessException e)
                {
                    ConnectOnError(e,"請檢查其他程式是否占用Port");
                }
                catch (ArgumentOutOfRangeException e)
                {
                    ConnectOnError(e, "參數錯誤，請檢查連線參數");
                }
                catch (System.IO.IOException e)
                {
                    ConnectOnError(e, "無效的連線參數或連接埠處於無效狀態");
                }
            }
            
        }
        private void ConnectOnError(Exception e,string helperText)
        {
            ShowDialog("連線失敗",
                        string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", e.Message, helperText),
                        "重新連線",
                        () => { currentTab.Connect(); },
                        PackIconKind.CloseCircleOutline);
            currentTab.onConnectFail();
            currentTab.onDissconnect();
        }

        private void ShowDialog(string title,string content,string redoButtonText,Action buttonRedoAction,MaterialDesignThemes.Wpf.PackIconKind icon)
        {
            PageDialog dialog = new PageDialog();
            dialog.InfoTitle = title;
            dialog.InfoContent = content;
            dialog.Icon = icon;
            dialog.Tab = currentTab;
            dialog.Button_Redo.Content = redoButtonText;
            dialog.ButtonRedoAction = buttonRedoAction;
            ((MainWindow)App.Current.MainWindow).ConnectionTabHelper.ShowDialogOnReceiveWindow(currentTab,dialog.PopupDialog);
        }

        public void Close()
        {
            try
            {
                
                App.Current.Dispatcher.InvokeAsync (()=>{ SerialPort.Close(); });
                PageSetting pageSetting = currentTab.ConnectionType.AdvanceSettingsPage as PageSetting;
                pageSetting.ReleasePort (currentTab);
            }
            catch (Exception e)
            {

            }
            currentTab.onDissconnect();
        }

        public void Stop()
        {
            
        }

        public void StartReading()
        {
            

        }
        public void StoptReading()
        {

        }
        public void SendData(byte[] data,bool async=true)
        {
            if (SerialPort.IsOpen == false)
                return;
            if (async)
                SerialPort.BaseStream.WriteAsync(data, 0, data.Length);
            else
                SerialPort.BaseStream.Write(data, 0, data.Length);
            SerialPort.DtrEnable = true;
        }

        private void DataReceivedHandler(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //string indata = sp.ReadExisting();

            //int currentTime = timer.Elapsed.Milliseconds;

            DateTime currentTimeCP = DateTime.Now;

            int size = SerialPort.BytesToRead;
            byte[] ReceiveBytes = new byte[size];
            SerialPort.Read(ReceiveBytes, 0, size);

            //Debug.Write("/" + currentTimeCP.Millisecond + "/" + " Data Received:");
            //Debug.WriteLine(temp);
            //currentTab.parentWindow.connectionTabHelper.AddNewText(Color.FromRgb(0,0,0),temp);
            currentTab.ShowData(ReceiveBytes, DateTime.Now);
            toolBarSettings.ReceiveWindowTextUpdated = true;
            //currentTab.ReceivePage.Dispatcher.InvokeAsync(()=> {currentTab.ReceivePage.Receive_Text.AppendText(String.Format("[{0}]{1}\n", currentTimeCP.ToString("MM/dd HH:mm:ss"), temp)); });

        }

        //
        public bool AllowApplySettingsWithoutClose
        {
            get
            {
                if (Connected)
                {
                    Settings ConnectionSettings = (Settings)currentTab.ToolBarSetting.ConnectionSettings;
                    String PortName = String.Format(CultureInfo.InvariantCulture, "COM{0}", ConnectionSettings.ComPort);
                    if (SerialPort.PortName != PortName)
                        return false;
                }
                return true;
            }
        }
    }
}
