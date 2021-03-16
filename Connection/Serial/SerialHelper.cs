using System;
using System.Diagnostics;
using System.Windows;
using System.IO.Ports;
using System.Globalization;
using EasyCom.General;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace EasyCom.Connection.Serial
{
    public partial class SerialHelper : IConnection
    {
        //public SerialPortStream Serial;
        public SerialPort SerialPort { get; set; }
        private readonly ConnectionTabData currentTab;

        private EasyCom.Settings.ToolBarSetting toolBarSettings;

        public Stopwatch stopwatch1 = new Stopwatch();

        private Task readTask;
        private CancellationTokenSource cancellationTokenSource ;


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

            //SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            //readTask = new Task(Read);

            toolBarSettings = currentTab.ToolBarSetting;
            currentTab.ToolBarSetting.ConnectionSettings = (Settings)toolBarSettings.ConnectionSettings;
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
                
                Console.WriteLine(SerialPort.ReadTimeout);
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
                    cancellationTokenSource = new CancellationTokenSource();
                    readTask = new Task(Read);
                    readTask.Start();
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
            currentTab.OnDissconnect();
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
                cancellationTokenSource.Cancel();
                App.Current.Dispatcher.InvokeAsync (()=>{ SerialPort.Close(); });
                PageSetting pageSetting = currentTab.ConnectionType.AdvanceSettingsPage as PageSetting;
                pageSetting.ReleasePort(currentTab);
            }
            catch (Exception e)
            {

            }
            currentTab.OnDissconnect();
        }

        public void Stop()
        {
            
        }

        public bool SendData(byte[] data,bool async=true)
        {
            if (SerialPort.IsOpen == false)
                return false;
            try
            {
                if (async)
                    SerialPort.BaseStream.WriteAsync(data, 0, data.Length);
                else
                {
                    
                    SerialPort.BaseStream.Write(data, 0, data.Length);
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        
        private void DataReceivedHandler(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            int size = SerialPort.BytesToRead;
            byte[] ReceiveBytes = new byte[size];
            SerialPort.Read(ReceiveBytes, 0, size);

            currentTab.ShowData(ReceiveBytes, 0, size, DateTime.Now);
            toolBarSettings.ReceiveWindowTextUpdated = true;

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
        //For test
        public void Read()
        {
            bool isTimeout = false;
            int cursor = 0;
            bool finish = true;
            Stopwatch timeoutWatch = new Stopwatch();
            byte[] ReceiveBytes = new byte[1000];
            bool full = false;
            int count = 0;

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    int size = SerialPort.BytesToRead;
                    if (size != 0)
                    {
                        if (finish)
                        {
                            timeoutWatch.Restart();
                            finish = false;
                            cursor = 0;
                        }
                        if (cursor + size < ReceiveBytes.Length)
                        {
                            byte[] data = new byte[size];
                            SerialPort.Read(data, 0, size);
                            Buffer.BlockCopy(data, 0, ReceiveBytes, cursor, size);
                            cursor += size;
                            count += 1;
                        }
                        else
                            full = true;
                        
                    }
                    if (timeoutWatch.IsRunning && !finish||full)
                    {
                        
                        timeoutWatch.Stop();
                        long elapsed = timeoutWatch.ElapsedMilliseconds;
                        if (timeoutWatch.ElapsedMilliseconds >= SerialPort.ReadTimeout || full)
                        {
                           
                            count = 0;
                            full = false;
                            finish = true;
                            currentTab.ShowData(ReceiveBytes, 0, cursor, DateTime.Now);
                            toolBarSettings.ReceiveWindowTextUpdated = true;
                        }
                        else
                        {
                            timeoutWatch.Start();
                        }
                    }
                }
                catch (TimeoutException) {
                    Console.WriteLine("timeout");
                }
                Thread.Sleep(1);
            }
        }
    }
}
