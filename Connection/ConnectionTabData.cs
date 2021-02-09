
using EasyCom.Connection;
using EasyCom.Settings;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace EasyCom
{
    public class ConnectionTabData
    {
        private MainWindow parentWindow;
        private ConnectionTabHelper tabHelper;

        private ConnectionTabItem tabItem = null;
        private ConnectionType connectionType = null;
        public IConnection ConnectionObject { get; set; }=null;

        public ToolBarSetting ToolBarSetting { get; set; } = new ToolBarSetting();

        public ConnectionTabData(ConnectionTabHelper connectionTabHelper)
        {
            this.tabHelper = connectionTabHelper;
            
            this.parentWindow = connectionTabHelper.CurrentWindow;
            //this.ReceivePage = page_ConnectionTab;
            //Frame ReceiveFrame = new Frame();

            //ReceiveFrame.Content = ReceivePage;
            this.TabItem = new ConnectionTabItem(this);
            TabItem.Content = tabHelper.ReceivePageFrame;
            SetDefault();
        }

        public void SetDefault()
        {
            MainWindowOption mainWindowOption = parentWindow.Options;
            ToolBarSetting.Connected = false;
            
            ToolBarSetting.ConnectionSettings = null;

            ToolBarSetting.ReceiveAutoSpilt=true;
            ToolBarSetting.ReceiveShowTime=true;
            ToolBarSetting.ReceiveLineEnding=mainWindowOption.LineEndingTypeList.ElementAt(1);
            ToolBarSetting.ReceiveTimeOut=20;
            ToolBarSetting.ReceiveDecodeType= mainWindowOption.DecodingTypeList.ElementAt(1);
            
            ToolBarSetting.SendLineEnding= mainWindowOption.LineEndingTypeList.ElementAt(1);
            ToolBarSetting.SendHex=false;
            ToolBarSetting.SendShowOnReceive=true;
            
            ToolBarSetting.SendAutoSenderEnable=false;
            ToolBarSetting.SendAutoSenderInterval = 1;
            ToolBarSetting.SendAutoSenderAmountEnable = false;
            ToolBarSetting.SendAutoSenderAmount=0;
            
            ToolBarSetting.SendText="";
            ToolBarSetting.SendPath="";

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
            if (ConnectionObject is null || ConnectionObject.GetType()!=ConnectionType.ConnectionObjectType)
                CreateConnectionInstance();
            ConnectionObject.Open();
        }

        public bool ApplySetting()
        {
            IConnectionSettings newConnectionSetting;
            IConnectionSettings originalConnectionSetting = ToolBarSetting.ConnectionSettings;
            Type t = ((IPageSetting)connectionType.AdvanceSettingsPage).settingsStructType;
            newConnectionSetting = Activator.CreateInstance(t) as IConnectionSettings;
            if (!SaveSettingFromAdvancedSettingPage(newConnectionSetting))
            {
                return false;
            }
            ToolBarSetting.ConnectionSettings = newConnectionSetting;
            bool needClose = !ConnectionObject.AllowApplySettingsWithoutClose;
            if (needClose)
                ConnectionObject.Close();
            bool applySuccessful = ConnectionObject.ApplySettings();
            if (!needClose)
            {
                if (!applySuccessful)
                {
                    ToolBarSetting.ConnectionSettings = originalConnectionSetting;
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
                //CreateConnectionInstance();
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
                //((IPageSetting)connectionType.AdvanceSettingsPage).SettingsRestore(toolBarSetting.ConnectionSettings);
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

        public void onDissconnect()
        {
            TabItem.Available = false;
            parentWindow.Button_Connection_Connect_Available = false;
            ToolBarSetting.Connected = false;

            
        }

        public void ApplyOnFail()
        {
            parentWindow.SettingChangedCallBack(null,true);
        }

        public void onError()
        {

        }

        public void ShowData(byte[] data,DateTime time)
        {
            String ConvertedData = Encoding.UTF8.GetString(data);
            AppendTextToReceiveWindow(true,ConvertedData,time);

        }

        public void SendData(string data,DateTime time)
        {
            if (ConnectionObject != null && ConnectionObject.Connected)
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                ToolBarSetting.ReceiveWindowTextUpdated = true;
                ConnectionObject.SendData(dataBytes);
                if (ToolBarSetting.SendShowOnReceive)
                {
                    AppendTextToReceiveWindow(false, data, time);
                }
            }
        }

        public void AppendTextToReceiveWindow(string data)
        {
            
        }

        public void AppendTextToReceiveWindow(bool input,string data, DateTime time)
        {
            
            if (ToolBarSetting.ReceiveAutoSpilt)
            {
                if (ToolBarSetting.ReceiveShowTime)
                {
                    ToolBarSetting.ReceiveWindowText.Append(time.ToString("[MM H:mm:ss.fff]",CultureInfo.InvariantCulture));
                    ToolBarSetting.ReceiveWindowText.Append(input ? "⊙ " : "⊕ ");
                    
                }
                ToolBarSetting.ReceiveWindowText.Append(data);
                ToolBarSetting.ReceiveWindowText.Append(ToolBarSetting.ReceiveLineEnding.Value);
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
                Debug.Print("Clear");
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
    }
}
