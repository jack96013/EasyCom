
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

        public bool isSelected = false;

        public ToolBarSetting toolBarSetting = new ToolBarSetting();

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
            toolBarSetting.Connected = false;
            
            toolBarSetting.ConnectionSettings = null;

            toolBarSetting.ReceiveAutoSpilt=true;
            toolBarSetting.ReceiveShowTime=true;
            toolBarSetting.ReceiveLineEnding=mainWindowOption.LineEndingTypeList.ElementAt(1);
            toolBarSetting.ReceiveTimeOut=20;
            toolBarSetting.ReceiveDecodeType= mainWindowOption.DecodingTypeList.ElementAt(1);
            
            toolBarSetting.SendLineEnding= mainWindowOption.LineEndingTypeList.ElementAt(1);
            toolBarSetting.SendHex=false;
            toolBarSetting.SendShowOnReceive=true;
            
            toolBarSetting.SendAutoSenderEnable=false;
            toolBarSetting.SendAutoSenderInterval = 1;
            toolBarSetting.SendAutoSenderAmountEnable = false;
            toolBarSetting.SendAutoSenderAmount=0;
            
            toolBarSetting.SendText="";
            toolBarSetting.SendPath="";

            toolBarSetting.ReceiveWindowText = new StringBuilder();
            toolBarSetting.ReceiveWindowText.Capacity = 50;

            toolBarSetting.ReceiveWindowTextUpdated = false;
            toolBarSetting.FlowDocument = new FlowDocument();
            toolBarSetting.FlowDocument.Blocks.Add(new Paragraph());

            toolBarSetting.ReceiveWindowFreeze = false;
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

        public void Connect()
        {
            //Set and detect if it need create new instance or use the older
            
            if (toolBarSetting.ConnectionSettings != null)
            {
                ((IPageSetting)this.ConnectionType.AdvanceSettingsPage).GetSetting(toolBarSetting.ConnectionSettings);
            }
            ConnectionObject.Open();
        }

        public void ApplySetting()
        {
            ((IPageSetting)this.ConnectionType.AdvanceSettingsPage).GetSetting(toolBarSetting.ConnectionSettings);
            if (!ConnectionObject.AllowApplySettingsWithoutClose)
            {
                ConnectionObject.Close();
                ConnectionObject.ApplySettings();
                ConnectionObject.Open();
            }
            else
                ConnectionObject.ApplySettings();
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
                CreateConnectionInstance();
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
                if (toolBarSetting.ConnectionSettings == null || toolBarSetting.ConnectionSettings.GetType() != t)
                {
                    Debug.WriteLine("CreateNewSetting " + TabItem.Title);
                    toolBarSetting.ConnectionSettings = Activator.CreateInstance(t);

                    Debug.WriteLine("Set Default");
                    ((IPageSetting)connectionType.AdvanceSettingsPage).SetSettingDefault(toolBarSetting.ConnectionSettings);
                }
                ((IPageSetting)connectionType.AdvanceSettingsPage).SettingsRestore(toolBarSetting.ConnectionSettings);
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
            toolBarSetting.Connected = true;

            Debug.WriteLine(String.Format("Tab \"{0}\" {1} Connected", TabItem.Title, this.connectionType.Name));

        }

        public void onConnectFail()
        {

        }

        public void onDissconnect()
        {
            TabItem.Available = false;
            parentWindow.Button_Connection_Connect_Available = false;
            toolBarSetting.Connected = false;
            //CurrentWindow.Frame_Connection_Setting.IsEnabled = true;
            //CurrentWindow.Button_Connection_Connect.Content = "連線";
            //Connected = false;
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
                toolBarSetting.ReceiveWindowTextUpdated = true;
                ConnectionObject.SendData(dataBytes);
                if (toolBarSetting.SendShowOnReceive)
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
            
            if (toolBarSetting.ReceiveAutoSpilt)
            {
                if (toolBarSetting.ReceiveShowTime)
                {
                    toolBarSetting.ReceiveWindowText.Append(time.ToString("[MM H:mm:ss.fff]",CultureInfo.InvariantCulture));
                    toolBarSetting.ReceiveWindowText.Append(input ? "⊙ " : "⊕ ");
                    
                }
                toolBarSetting.ReceiveWindowText.Append(data);
                toolBarSetting.ReceiveWindowText.Append(toolBarSetting.ReceiveLineEnding.Value);
            }
            else
            {
                toolBarSetting.ReceiveWindowText.Append(data);
            }

            Debug.WriteLine(toolBarSetting.ReceiveWindowText.Length);
            //Debug.WriteLine (toolBarSetting.ReceiveWindow_Text.Length);
            if (toolBarSetting.ReceiveWindowText.Length > 30000)
            {
                toolBarSetting.ReceiveWindowText.Remove(0, toolBarSetting.ReceiveWindowText.Length- 30000);
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
            this.TabItem.Focus();
            this.tabHelper.SelectedTab = this;
        }
    }
}
