using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace EasyCom
{
    public class ControlCommandHandle
    {
        private MainWindow mainWindow = ((MainWindow)App.Current.MainWindow);
        private ConnectionTabHelper ConnectionTabHelper;
        private ConnectionTabData SelectedConnectionTab;

        private Action ParseFinishCB = null;

        private bool StartUp = false;
        private List<Action> TaskRunAfterStartUpList = new List<Action>();
        public ControlCommandHandle(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            ConnectionTabHelper = mainWindow.ConnectionTabHelper;
        }

        public void StartUpCommandHandle()
        {
            Dictionary<string, string> startUpArgs = ((App)App.Current).CommandData;
            StartUp = true;
            if (startUpArgs.Count != 0)
            {
                ConnectionTabHelper.AutoNewTab = false;
                ParseFinishCB = () =>
                {
                    this.mainWindow.Dispatcher.InvokeAsync(() => { SelectedConnectionTab?.Focus(); 
                    }); //focus and restore toolbar settings
                    ParseFinishCB = null;
                    ConnectionTabHelper.OneTabExistCheck();
                    ConnectionTabHelper.AutoNewTab = true;
                    
                    foreach (Action task in TaskRunAfterStartUpList)
                    {
                        task.Invoke();
                    }
                    TaskRunAfterStartUpList.Clear();
                    StartUp = false;
                };
                this.CommandsParse(startUpArgs);
                

            }
            else
            {
                ConnectionTabHelper.AutoNewTab = true;
                ConnectionTabHelper.OneTabExistCheck();
                StartUp = false;
            }
            

        }

        public void CommandsParse(Dictionary<string, string> keyValuePairs)
        {
            Task.Run(() =>
            {
                
                foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
                {
                    CommandExe(keyValuePair.Key, keyValuePair.Value);
                    //Debug.WriteLine(String.Format("Key:{0},Value:{1}", keyValuePair.Key, keyValuePair.Value));
                }

                ParseFinishCB?.Invoke();
                
            });
        }
        public void CommandParse(string key, string value)
        {
            CommandExe(key, value);
            mainWindow.Dispatcher.Invoke(() => { ((IPageSetting)SelectedConnectionTab.ConnectionType.AdvanceSettingsPage).SettingsRestore(SelectedConnectionTab.ToolBarSetting.ConnectionSettings); });
        }

        private void CommandExe(string key, string value)
        {
            string report = "";
            if (StrCompare(key, "exit"))
                mainWindow.Dispatcher.Invoke(() => { App.Current.Shutdown(); });
            else if (StrCompare(key, "send"))
            {
                if (value == null)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.Text_Send_Click(null, null);
                    });
                }
                else
                {
                    if (value.ElementAt(0) == '"' && value.ElementAt(value.Length - 1) == '"')
                        ;
                    int newlineMark = value.IndexOf("\\n");

                    value = value.Replace("\\n", "\n");
                    value = value.Replace("\\r", "\r");
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        SelectedConnectionTab.SendData(value, DateTime.Now);
                    });
                }


            }
            else if (StrCompare(key, "clear"))
            {
                if (value == null)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.Button_ReceiveWindow_Clear_Click(null, null);

                    });

                }
            }
            else if (StrCompare(key, "freeze"))
            {
                if (value == null)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {

                    });
                }
            }


            else if (StrCompare(key, "serial"))
            {
                if (SelectedConnectionTab == null)
                {
                    Debug.WriteLine("choose serial");

                    ConnectionTabData connection = null;
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine("NewTab");
                        connection = ConnectionTabHelper.NewTab();
                        connection.TabItem.Title = "ArduBlockly";
                        connection.ToolBarSetting.SendText = "測試";
                        SelectedConnectionTab = connection;
                        SelectedConnectionTab.ConnectionTypeChoose(0);

                    });
                }
                ((Connection.Serial.Settings)SelectedConnectionTab.ToolBarSetting.ConnectionSettings).ComPort = int.Parse(value);
                Debug.WriteLine("set comport");
            }

            else if (StrCompare(key, "disconnect"))
            {
                mainWindow.Dispatcher.Invoke(() => { SelectedConnectionTab.Disconnect(); });
            }
            else if (StrCompare(key, "connect"))
            {
                Debug.WriteLine("Connect");
                mainWindow.Dispatcher.Invoke(() => { SelectedConnectionTab.Connect(); });
            }
            else if (StrCompare(key, "text"))
            {
                if (value != null)
                {
                    value = value.Replace("\\n", "\n");
                    value = value.Replace("\\r", "\r");
                    mainWindow.Dispatcher.Invoke(() => { mainWindow.TextBox_Send_Text.Text = value; });
                }
            }
            else if (StrCompare(key, "auto_spilt"))
            {
                bool result = false;
                bool successful = bool.TryParse(value, out result);
                if (successful)
                {
                    mainWindow.Dispatcher.Invoke(() => { mainWindow.Toggle_Receive_AutoSpilt.IsChecked = result; });
                }
            }
            else if (StrCompare(key, "show_time"))
            {
                bool result = false;
                bool successful = bool.TryParse(value, out result);
                if (successful)
                {
                    mainWindow.Dispatcher.Invoke(() => { mainWindow.Toggle_Receive_ShowTime.IsChecked = result; });
                }
            }
            else if (StrCompare(key, "next_line"))
            {

                mainWindow.Dispatcher.Invoke(() => { mainWindow.Button_ReceiveWindow_PrintNewLine_Click(null, null); });

            }
            else if (StrCompare(key, "window"))
            {
                System.Windows.WindowState state = System.Windows.WindowState.Normal;
                if (StrCompare(value, "max"))
                {
                    state = System.Windows.WindowState.Maximized;
                }
                else if (StrCompare(value, "min"))
                {
                    state = System.Windows.WindowState.Minimized;
                }
                else if (StrCompare(value, "normal"))
                {
                    state = System.Windows.WindowState.Normal;
                }
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.WindowState = state;
                });
            }
            else
            {
                if (SelectedConnectionTab != null)
                    SelectedConnectionTab.ConnectionObject.CommandHandle(key, value, report);
            }
        }

        private static bool StrCompare(string src, string target)
        {
            if (src is null)
                return false;
            return src.Equals(target, StringComparison.InvariantCulture);
        }
    }
}