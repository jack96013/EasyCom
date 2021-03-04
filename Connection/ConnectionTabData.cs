
using EasyCom.Connection;
using EasyCom.Settings;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace EasyCom
{
    public class ConnectionTabData
    {
        private MainWindow parentWindow;
        private ConnectionTabHelper tabHelper;

        private ConnectionTabItem tabItem = null;
        private ConnectionType connectionType = null;
        public IConnection ConnectionObject { get; set; } = null;
        public IConnectionSettings ConnectionSettings { get; set; } //For Connected ConnectionObject

        public ToolBarSetting ToolBarSetting { get; set; } = new ToolBarSetting();
        public AutoSendRobot AutoSendRobot { get; } = null;


        public ConnectionTabData(ConnectionTabHelper connectionTabHelper)
        {
            this.tabHelper = connectionTabHelper;

            this.parentWindow = connectionTabHelper.CurrentWindow;
            //this.ReceivePage = page_ConnectionTab;
            //Frame ReceiveFrame = new Frame();

            //ReceiveFrame.Content = ReceivePage;
            this.TabItem = new ConnectionTabItem(this);
            AutoSendRobot = new AutoSendRobot(this);
            AutoSendRobot.DataChanged = parentWindow.UpdateAutoSenderInfo;

            TabItem.Content = tabHelper.ReceivePageFrame;
            SetDefault();
        }

        public void SetDefault()
        {
            MainWindowOption mainWindowOption = parentWindow.Options;
            ToolBarSetting.Connected = false;

            ToolBarSetting.ConnectionSettings = null;

            ToolBarSetting.ReceiveAutoSpilt = true;
            ToolBarSetting.ReceiveShowTime = true;
            ToolBarSetting.ReceiveLineEnding = mainWindowOption.LineEndingTypeList.ElementAt(1);
            ToolBarSetting.ReceiveTimeOut = 20;
            ToolBarSetting.ReceiveEncodingType = mainWindowOption.EncodingTypeList.ElementAt(1);

            ToolBarSetting.SendLineEnding = mainWindowOption.LineEndingTypeList.ElementAt(0);
            ToolBarSetting.SendEncodingType = mainWindowOption.EncodingTypeList.ElementAt(1);
            ToolBarSetting.SendHex = false;
            ToolBarSetting.SendShowOnReceive = true;

            ToolBarSetting.SendAutoSenderEnable = false;
            ToolBarSetting.SendAutoSenderInterval = 10;
            ToolBarSetting.SendAutoSenderAmountEnable = false;
            ToolBarSetting.SendAutoSenderAmount = 0;

            ToolBarSetting.SendText = "";
            ToolBarSetting.SendPath = "";
            ToolBarSetting.SendFileBufferSize = 256;
            ToolBarSetting.SendFileInterval = 10;

            ToolBarSetting.ReceiveWindowText = new StringBuilder();
            ToolBarSetting.ReceiveWindowText.Capacity = 50;

            ToolBarSetting.ReceiveWindowTextUpdated = false;
            ToolBarSetting.FlowDocument = new FlowDocument();
            ToolBarSetting.FlowDocument.Blocks.Add(new Paragraph());

            ToolBarSetting.ReceiveWindowFreeze = false;
        }

        public ConnectionType ConnectionType
        {
            set
            {
                connectionType = value;
            }
            get
            {
                return connectionType;
            }
        }

        public bool Connected
        {
            get
            {
                if (ConnectionObject != null)
                {
                    return ConnectionObject.Connected;
                }
                else
                {
                    return false;
                }

            }
        }

        public ConnectionTabItem TabItem { get => tabItem; set => tabItem = value; }
        public bool SaveSettingFromAdvancedSettingPage()
        {
            bool a = SaveSettingFromAdvancedSettingPage(ToolBarSetting.ConnectionSettings);
            //Console.WriteLine(((Connection.Serial.Settings)ToolBarSetting.ConnectionSettings).Info());
            return a;
        }
        private bool SaveSettingFromAdvancedSettingPage(object setting)
        {
            if (tabHelper.SelectedTab == this)
            {
                ((IPageSetting)ConnectionType.AdvanceSettingsPage).GetSetting(setting);

                return true;
            }
            return false;

        }

        public void Connect()
        {
            ConnectionSettings = ToolBarSetting.ConnectionSettings;
            //Console.WriteLine(((Connection.Serial.Settings)ConnectionSettings).Info()); 
            if (ConnectionObject is null || ConnectionObject.GetType() != ConnectionType.ConnectionObjectType)
                CreateConnectionInstance();
            ConnectionObject.Open();
        }

        public bool ApplySetting()
        {
            IConnectionSettings newConnectionSetting;
            IConnectionSettings originalConnectionSetting = ConnectionSettings;
            Type t = ((IPageSetting)connectionType.AdvanceSettingsPage).settingsStructType;
            newConnectionSetting = Activator.CreateInstance(t) as IConnectionSettings;
            if (!SaveSettingFromAdvancedSettingPage(newConnectionSetting))
            {
                return false;
            }
            ConnectionSettings = newConnectionSetting;
            bool needClose = !ConnectionObject.AllowApplySettingsWithoutClose;
            if (needClose)
                ConnectionObject.Close();

            bool applySuccessful = ConnectionObject.ApplySettings();
            if (!needClose)
            {
                if (!applySuccessful)
                {
                    ConnectionSettings = originalConnectionSetting;
                }
            }
            else
                ConnectionObject.Open();
            return applySuccessful;
        }

        public void ReConnect()
        {
            Disconnect();
            Connect();
        }

        //選單選擇的連線型別(尚未連線)，建立物件儲存設定值
        public void ConnectionTypeChoose(int ConnectionTypeIndex)
        {
            if (ConnectionTypeIndex != -1)
            {
                this.ConnectionType = parentWindow.Options.ConnectionTypes.ElementAt(ConnectionTypeIndex);
            }
            else
                this.ConnectionType = null;

            if (ConnectionTypeIndex != -1)
            {
                CreateConnectionSettingsInstance(parentWindow.Options.ConnectionTypes.ElementAt(ConnectionTypeIndex));
            }
        }

        public void CreateConnectionInstance()
        {
            //If the new connection type is same as the older,don't create new one
            if (connectionType.ConnectionObjectType != null)
                ConnectionObject = (IConnection)Activator.CreateInstance(connectionType.ConnectionObjectType, this);
        }

        public void CreateConnectionSettingsInstance(ConnectionType connectionType)
        {
            if (connectionType.AdvanceSettingsPage != null)
            {
                Type t = ((IPageSetting)connectionType.AdvanceSettingsPage).settingsStructType;
                if (ToolBarSetting.ConnectionSettings == null || ToolBarSetting.ConnectionSettings.GetType() != t)
                {
                    Debug.WriteLine("CreateNewSetting " + TabItem.Title);
                    ToolBarSetting.ConnectionSettings = Activator.CreateInstance(t) as IConnectionSettings;

                    Debug.WriteLine("Set Default");
                    ((IPageSetting)connectionType.AdvanceSettingsPage).SetSettingDefault(ToolBarSetting.ConnectionSettings);
                }
                ((IPageSetting)connectionType.AdvanceSettingsPage).SettingsRestore(ToolBarSetting.ConnectionSettings);
            }
        }


        public void Disconnect()
        {
            if (ConnectionObject != null)
            {
                ConnectionObject.Close();
            }
        }


        public void onConnectSuccessful()
        {
            TabItem.Available = true;
            parentWindow.Button_Connection_Connect_Available = true;
            ToolBarSetting.Connected = true;

            Debug.WriteLine(String.Format("Tab \"{0}\" {1} Connected", TabItem.Title, this.connectionType.Name));

        }

        public void onConnectFail()
        {

        }

        public void OnDissconnect()
        {
            TabItem.Available = false;
            parentWindow.Button_Connection_Connect_Available = false;
            ToolBarSetting.Connected = false;

            //ToolBarSetting.SendAutoSenderEnable = false;
            AutoSendRobot.Stop();
            parentWindow.UpdateAutoSenderInfo(null,null);

        }

        public void ApplyOnFail()
        {
            parentWindow.SettingChangedCallBack(true);
        }

        public void onError()
        {

        }

        public void ShowData(byte[] data, DateTime time)
        {
            //String ConvertedData = Encoding.UTF8.GetString(data);

            string ConvertedData;
            if (ToolBarSetting.ReceiveEncodingType.Name == "HEX")
            {
                ConvertedData = BitConverter.ToString(data).Replace("-", " ");
            }
            else
                ConvertedData = ToolBarSetting.ReceiveEncodingType.Value.GetString(data);
            AppendTextToReceiveWindow(true, ConvertedData, time);

        }

        public bool SendData(string data, DateTime time,bool async=true)
        {
            
            if (ConnectionObject != null && ConnectionObject.Connected)
            {
                byte[] dataBytes;
                if (ToolBarSetting.SendEncodingType.Name != "HEX")
                {
                    if (ToolBarSetting.SendLineEnding.Value != null)
                    {
                        data += ToolBarSetting.SendLineEnding.Value;
                    }
                    dataBytes = ToolBarSetting.SendEncodingType.Value.GetBytes(data);
                }
                else
                {
                    if (data.EndsWith(" "))
                    {
                        data = data.Substring(0,data.Length-1);
                    }
                    
                    data = data.Replace("0x","");
                    data = data.Replace("h", "");
                    if (ToolBarSetting.SendLineEnding.Value != null)
                    {
                        data += ToolBarSetting.SendLineEnding.Value;
                    }
                    data = data.Replace("\r", " 0D");
                    data = data.Replace("\n", " 0A");

                    bool successful = StringToByteArray(data,out dataBytes);
                    if (!successful)
                        return false;
                }
                
                ToolBarSetting.ReceiveWindowTextUpdated = true;
                bool success = ConnectionObject.SendData(dataBytes, async);
                if (ToolBarSetting.SendShowOnReceive)
                {
                    AppendTextToReceiveWindow(false, data, time);
                }
                return success;
            }
            return false;
        }

        public static bool StringToByteArray(string hex,out byte[] array)
        {
            array = new byte[hex.Length/2];
            bool successful = true;
            if (hex.Length % 3 != 2)
                return false;
            for (int i = 0; i < hex.Length; i++)
            {
                if (i % 3 == 2)
                {
                    if (hex[i] != ' ')
                    {
                        successful = false;
                        break;
                    }
                }
                else if (i % 3 ==0)
                {
                    string hexValue = hex.Substring(i, 2);
                    Console.WriteLine(hexValue);
                    Match match = Regex.Match(hexValue, "^[A-Fa-f0-9]+$");
                    if (match.Success)
                    {
                        array[i / 2] = Convert.ToByte(hexValue, 16);
                    }
                    else
                    {
                        successful = false;
                        break;
                    }
                }
            }
            return successful;
        }


        public void SendFile()
        {
            Console.WriteLine(ToolBarSetting.SendPath);
            if (ConnectionObject == null || !ConnectionObject.Connected)
            {
                return;
            }
            Task.Factory.StartNew(() => {
                using (FileStream fs = File.Open(ToolBarSetting.SendPath,FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    DateTime startTime = System.DateTime.Now;
                    BufferedStream BufferedStreamInput = new BufferedStream(fs);

                    // Add some information to the file.
                    //fs.Write(info, 0, info.Length);
                    AppendTextToReceiveWindow(false, string.Format("開始發送檔案共 {0} Bytes",fs.Length) , DateTime.Now);
                    ToolBarSetting.ReceiveWindowTextUpdated = true;

                    int split = ToolBarSetting.SendFileBufferSize;
                    long index = 0;
                    byte[] dataArray = new byte[split];

                    long lenSplit = fs.Length / 100;
                    long lenSplitCur = lenSplit;
                    int percent=0;

                    int intByte = 0;

                    bool finish = false;
                    while(!finish)
                    {
                        if (!ConnectionObject.Connected)
                            return;
                        //透過緩衝輸入物件，將資料讀到 陣列
                        //一次讀取 16 個位元組長度
                        intByte = BufferedStreamInput.Read(dataArray, 0, split);
                        //Console.WriteLine(intByte);
                        //同樣的透過緩衝物件輸出資料

                        //將緩衝區資料清除
                        BufferedStreamInput.Flush();
                        index += intByte;
                        if (intByte != split)
                        {
                            BufferedStreamInput.Close();
                            finish = true;
                        }
                        if (index >= lenSplitCur)
                        {
                            lenSplitCur += lenSplit;
                            percent++;
                            Console.WriteLine(percent);
                            AppendTextToReceiveWindow(false, string.Format(CultureInfo.InvariantCulture, "檔案發送 {0}%", percent),DateTime.Now);
                            ToolBarSetting.ReceiveWindowTextUpdated = true;

                        }
                        if (finish)
                        {
                            Array.Resize(ref dataArray,intByte);
                        }                        
                        ConnectionObject.SendData(dataArray,false);
                        
                        Thread.Sleep(ToolBarSetting.SendFileInterval);
                    }
                    AppendTextToReceiveWindow(false, string.Format(CultureInfo.InvariantCulture, "發送完成! 共耗時 {0}", DateTime.Now.Subtract(startTime).ToString()), DateTime.Now);
                    
                }
                Console.WriteLine("finish\n");
            },CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        public void AppendTextToReceiveWindow(string data)
        {
            
        }


        public void AppendTextToReceiveWindow(bool input,string data, DateTime time)
        {
            
            if (ToolBarSetting.ReceiveAutoSpilt)
            {
                if (ToolBarSetting.ReceiveWindowText.Length != 0)
                    ToolBarSetting.ReceiveWindowText.Append("\n");
                if (ToolBarSetting.ReceiveShowTime)
                {
                    ToolBarSetting.ReceiveWindowText.Append(time.ToString("[MM H:mm:ss.fff]",CultureInfo.InvariantCulture));
                    ToolBarSetting.ReceiveWindowText.Append(input ? "⊙ " : "⊕ ");
                    
                }
                
                ToolBarSetting.ReceiveWindowText.Append(data);
            }
            else
            {
                ToolBarSetting.ReceiveWindowText.Append(data);
                
            }

            Debug.WriteLine(ToolBarSetting.ReceiveWindowText.Length);
            //Debug.WriteLine (toolBarSetting.ReceiveWindow_Text.Length);
            if (ToolBarSetting.ReceiveWindowText.Length > 30000)
            {
                ToolBarSetting.ReceiveWindowText.Remove(0, ToolBarSetting.ReceiveWindowText.Length- 30000);
            }

            //TEST

            /*
            parentWindow.Dispatcher.Invoke(()=>
            {
                Paragraph paragraph = (Paragraph)toolBarSetting.flowDocument.Blocks.ElementAt(0);

                Run myRun1 = new Run() { Foreground = new SolidColorBrush(Colors.Red), Text = time.ToString("[MM H:mm:ss.fff] ") };
                Run myRun2 = new Run() { Text = data };
                paragraph.Inlines.Add(myRun1);
                paragraph.Inlines.Add(myRun2);
            });
            */
        }

        public void Focus()
        {
            //this.TabItem.Focus();
            this.tabHelper.SelectedTab = this;
        }

        public bool IsSelected
        {
            get {
                return this.tabHelper.SelectedTab == this;
            }
        }
        
    }
}
