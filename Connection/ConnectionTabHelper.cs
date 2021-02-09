using EasyCom.General;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace EasyCom
{
    public class ConnectionTabHelper
    {
        public TabControl TabControl_ReceiveWindow;

        private Page_ConnectionTab receivePage;
        public Frame ReceivePageFrame { get; } = new Frame();

        private MainWindow currentWindow;

        private List<ConnectionTabData> connectionTabDataList = new List<ConnectionTabData>();

        private TabItem TabItem_Add = new TabItem() { Header = "+", Width = 30 };

        public DispatcherTimer ReceiveWindowRefreshTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };
        public ManualResetEvent ReceiveWindowRefreshLock = new ManualResetEvent(true);
        public ManualResetEvent ReceiveWindowRefreshLock_Show = new ManualResetEvent(true);

        private ConnectionTabData selectedTab = null;

        private bool autoNewTab = false;

        private int Count = 0;
        private TabItem previousSelectedTab = null;

        public ConnectionTabData CurrentTabData
        {

            get
            {
                if (TabControl_ReceiveWindow.SelectedItem != null && TabControl_ReceiveWindow.SelectedItem != TabItem_Add)
                    return ((ConnectionTabItem)TabControl_ReceiveWindow.SelectedItem).connectionTabData;
                else
                    return null;
            }
        }

        public Page_ConnectionTab ReceivePage { get => receivePage; set => receivePage = value; }
        public MainWindow CurrentWindow { get => currentWindow; set => currentWindow = value; }
        public bool AutoNewTab { get => autoNewTab; set => autoNewTab = value; }
        public ConnectionTabData SelectedTab
        {
            get => selectedTab; set
            {
                selectedTab = value;
                TabControl_ReceiveWindow.SelectedItem = selectedTab.TabItem;

            }
        }


        public ConnectionTabHelper(MainWindow currentWindow)
        {
            this.CurrentWindow = currentWindow;

            this.ReceivePage = new Page_ConnectionTab(this);

            this.TabControl_ReceiveWindow = currentWindow.TabControl_ReceiveWindow;
            this.TabControl_ReceiveWindow.Loaded += (s, e) => { };
            ReceivePageFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            ReceivePageFrame.Content = ReceivePage;
            TabControl_ReceiveWindow.Items.Add(TabItem_Add);

            ReceiveWindowRefreshTimer.Tick += ReceiveWindowRefreshTimer_Tick;
            ReceiveWindowRefreshTimer.Start();
            this.TabControl_ReceiveWindow.SelectionChanged += TabControl_ReceiveWindow_SelectionChanged;
        }

        private void ReceiveWindowRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (CurrentTabData != null && CurrentTabData.ToolBarSetting.ReceiveWindowTextUpdated)
            {
                ReceiveWindowRefreshLock.WaitOne();
                if (!CurrentTabData.ToolBarSetting.ReceiveWindowFreeze && CurrentTabData.ToolBarSetting.ReceiveWindowTextUpdated)
                {
                    ReceiveWindowRefreshLock_Show.Reset();
                    //  ReceivePage.TextBox_Receive.Text = CurrentTabData.toolBarSetting.ReceiveWindow_Text.ToString();

                    try
                    {
                        //ReceivePage.TextBox_Test.Text = CurrentTabData.toolBarSetting.ReceiveWindow_Text.ToString();
                        AddNewText(Color.FromArgb(0, 0, 0, 0), CurrentTabData.ToolBarSetting.ReceiveWindowText.ToString());

                        //ReceivePage.Receive_Text.Document = CurrentTabData.toolBarSetting.flowDocument;
                        CurrentTabData.ToolBarSetting.ReceiveWindowTextUpdated = false;
                    }
                    catch (Exception k)
                    {
                        Debug.WriteLine(k.ToString(), "Error");
                    }

                    if (IsVerticalScrollBarAtBottom)
                        ReceivePage.TextBox_Test.ScrollToEnd();
                    //ReceivePage.TextBox_Receive.CaretIndex = CurrentTabData.toolBarSetting.ReceiveWindow_Text.Length;
                    ReceiveWindowRefreshLock_Show.Set();
                }
            }
        }

        public bool IsVerticalScrollBarAtBottom
        {
            get
            {
                bool atBottom = false;

                this.ReceivePage.TextBox_Receive.Dispatcher.Invoke((Action)delegate
                {

                    double dVer = this.ReceivePage.TextBox_Test.VerticalOffset;       //獲取豎直滾動條滾動位置
                    double dViewport = this.ReceivePage.TextBox_Test.ViewportHeight;  //獲取豎直可滾動內容高度
                    double dExtent = this.ReceivePage.TextBox_Test.ExtentHeight;      //獲取可視區域的高度

                    if (dVer + dViewport >= dExtent)
                    {
                        atBottom = true;
                    }
                    else
                    {
                        atBottom = false;
                    }
                });

                return atBottom;
            }
        }
        //Detect there have any tab in tabControl, if not create new tab.
        public void OneTabExistCheck()
        {
            if (TabControl_ReceiveWindow.Items.Count == 1)
            {
                this.currentWindow.Dispatcher.Invoke(() => { NewTabAndSelect(); });
            }
        }


        private void TabControl_ReceiveWindow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            TabItem selectedTab = (TabItem)TabControl_ReceiveWindow.SelectedItem;
            if (previousSelectedTab != selectedTab)
            {
                Debug.WriteLine(TabControl_ReceiveWindow.Items.Count, "TabSelectionChanged");
                if (previousSelectedTab is ConnectionTabItem)
                {
                    ConnectionTabItem previous = previousSelectedTab as ConnectionTabItem; //get the previous tabitem
                    Debug.WriteLine("Prev : " + previous.Title);
                    SaveToolBarSettingsForPreviousTab(previous.connectionTabData);

                }
                if (selectedTab.Equals(TabItem_Add))
                {
                    if (AutoNewTab)
                    {
                        CurrentWindow.Dispatcher.InvokeAsync(() =>
                        {
                            NewTabAndSelect();
                        });
                    }
                }
                else if (selectedTab is ConnectionTabItem)
                {
                    ConnectionTabItem previous = previousSelectedTab as ConnectionTabItem; //get the previous tabitem


                    Debug.WriteLine("Next : " + ((ConnectionTabItem)TabControl_ReceiveWindow.SelectedItem).Title);
                    foreach (ConnectionTabData k in connectionTabDataList)
                    {
                        k.TabItem.Content = null;
                    }
                    CurrentWindow.Dispatcher.InvokeAsync(() => { ((ConnectionTabItem)TabControl_ReceiveWindow.SelectedItem).Content = ReceivePage; });
                    previousSelectedTab = (ConnectionTabItem)TabControl_ReceiveWindow.SelectedItem;
                    RestoreToolBarSettingsForCurrentTab();

                }
                previousSelectedTab = selectedTab;
            }
        }

        public ConnectionTabData NewTab()
        {
            ConnectionTabData newData = new ConnectionTabData(this);
            newData.TabItem.onClose = CloseTab;

            connectionTabDataList.Add(newData);

            //將建立好的加進分頁
            TabControl_ReceiveWindow.Items.Insert(TabControl_ReceiveWindow.Items.Count - 1, newData.TabItem);
            //如果分頁數為0新增
            newData.TabItem.Title = "New Connection" + Count;

            Count++;

            //newData.Focus();
            Debug.WriteLine("Sel+" + TabControl_ReceiveWindow.SelectedIndex);
            return newData;
        }

        public ConnectionTabData NewTabAndSelect()
        {
            ConnectionTabData newtab = NewTab();
            this.SelectedTab = newtab;
            return newtab;
        }

        // 按下關閉分頁時的動作
        public void CloseTab(Object sender)
        {
            ConnectionTabItem currentTabItem = (ConnectionTabItem)sender;


            int itemIndex = TabControl_ReceiveWindow.Items.IndexOf(currentTabItem);

            if (TabControl_ReceiveWindow.Items.Count - 1 == 1)
            {
                NewTabAndSelect();
            }
            //If the close item is the last tabitem in tabcontrol,focus on the last item.
            else if (itemIndex != -1 && itemIndex + 1 == TabControl_ReceiveWindow.Items.Count - 1)
            {
                ((ConnectionTabItem)TabControl_ReceiveWindow.Items.GetItemAt(itemIndex - 1)).Focus();
            }

            TabControl_ReceiveWindow.Items.Remove(currentTabItem);
            connectionTabDataList.Remove(currentTabItem.connectionTabData);
        }

        public void AddNewText(Color color, string text)
        {
            ReceivePage.testViewModel.text = text;
            CurrentWindow.Dispatcher.Invoke(() =>
            {
                //Paragraph paragraph = (Paragraph)ReceivePage.Receive_Text.Document.Blocks.ElementAt(0);
                //Run myRun1 = new Run();
                //myRun1.Text = "A RichTextBox with ";
                //paragraph.Inlines.Add(new Run("dd"));// .Inlines.Add("ddd");
                //Run dd = (Run)paragraph.Inlines.First();
                ///ReceivePage.Receive_Text.SelectionBrush = new SolidColorBrush(Colors.Green);
                //ReceivePage.Receive_Text.AppendText(text);

                //ReceivePage.Receive_Text.AppendText("dddd");

                //ReceivePage.TextBox_Receive.Text=text;

                ReceivePage.TextBox_Test.Text = text;
            });
        }

        private void SaveToolBarSettingsForPreviousTab(ConnectionTabData PreviousTabData)
        {
            if (PreviousTabData.ToolBarSetting.ConnectionSettings != null && PreviousTabData.ConnectionType.AdvanceSettingsPage != null)
            {
                Debug.WriteLine("Save " + PreviousTabData.TabItem.Title);
                //((IPageSetting)PreviousTabData.ConnectionType.AdvanceSettingsPage).GetSetting(PreviousTabData.ToolBarSetting.ConnectionSettings);
            }
        }
        /// <summary>
        /// Save toolbar settings to current tab
        /// </summary>
        public void RestoreToolBarSettingsForCurrentTab()
        {
            RestoreToolBarSettings(CurrentTabData);
        }
        /// <summary>
        /// Save toolbar settings
        /// </summary>
        /// <param name="tabData"></param>
        public void RestoreToolBarSettings(ConnectionTabData tabData)
        {
            //CurrentTabData.isSelected = true;
            Debug.WriteLine(CurrentTabData.TabItem.Title, "Restore");
            
            if (tabData == null)
                return;
            tabData.ToolBarSetting.ReceiveWindowTextUpdated = true;

            Settings.ToolBarSetting toolBarSettings = tabData.ToolBarSetting;
            
            CurrentWindow.Button_Connection_Connect_Available = toolBarSettings.Connected;
            

            CurrentWindow.Toggle_Receive_AutoSpilt.IsChecked = toolBarSettings.ReceiveAutoSpilt;
            CurrentWindow.Toggle_Receive_ShowTime.IsChecked = toolBarSettings.ReceiveShowTime;
            CurrentWindow.Combo_Receive_LineEnding.SelectedItem = toolBarSettings.ReceiveLineEnding;
            CurrentWindow.TextBox_Receive_Timeout.Text = toolBarSettings.ReceiveTimeOut.ToString(CultureInfo.InvariantCulture);
            CurrentWindow.Combo_Receive_DecodeType.SelectedItem = toolBarSettings.ReceiveDecodeType;

            CurrentWindow.Combo_Send_LineEnding.SelectedItem = toolBarSettings.SendLineEnding;
            CurrentWindow.Toggle_Send_Hex.IsChecked = toolBarSettings.SendHex;
            CurrentWindow.Toggle_Send_ShowOnReceive.IsChecked = toolBarSettings.SendShowOnReceive;

            CurrentWindow.CheckBox_AutoSender_Enable.IsChecked = toolBarSettings.SendAutoSenderEnable;
            CurrentWindow.TextBox_AutoSender_Interval.Text = toolBarSettings.SendAutoSenderInterval.ToString(CultureInfo.InvariantCulture);
            CurrentWindow.CheckBox_AutoSender_AmountEnable.IsChecked = toolBarSettings.SendAutoSenderAmountEnable;
            CurrentWindow.TextBox_AutoSender_Amount.Text = toolBarSettings.SendAutoSenderAmount.ToString(CultureInfo.InvariantCulture);

            CurrentWindow.TextBox_Send_Text.Text = toolBarSettings.SendText;
            CurrentWindow.TextBox_Send_Path.Text = toolBarSettings.SendPath;

            toolBarSettings.ReceiveWindowTextUpdated = true;

            if (toolBarSettings.ConnectionSettings != null && tabData.ConnectionType != null)
            {
                //Restor settings for AdvanceSettingPage 
                Debug.WriteLine(CurrentTabData.TabItem.Title, "Restore Connection Setting");
                ((IPageSetting)CurrentTabData.ConnectionType.AdvanceSettingsPage).SettingsRestore(toolBarSettings.ConnectionSettings);
                Debug.WriteLine(((Connection.Serial.Settings)toolBarSettings.ConnectionSettings).ComPort, "PORT");
            }
            CurrentWindow.ComboBox_Connection_Type.SelectedItem = tabData.ConnectionType;

            receivePage.PopupDialogHostReceive.ReplaceDialog(toolBarSettings.PopupDialogReceive);
        }

        public void ShowDialogOnReceiveWindow(ConnectionTabData tab, PopupDialog dialog)
        {

            if (tab is null || dialog is null)
                return;
            tab.ToolBarSetting.PopupDialogReceive = dialog;
            dialog.WindowStatus = PopupDialog.Status.Show;
            if (CurrentTabData == tab)
            {
                receivePage.PopupDialogHostReceive.Show(dialog);
            }
        }
        public void CloseDialogOnReceiveWindow(ConnectionTabData tab, PopupDialog dialog)
        {
            if (tab is null)
                return;
            tab.ToolBarSetting.PopupDialogReceive.Close();
            tab.ToolBarSetting.PopupDialogReceive = null;
        }


        public void RefreshReceiveWindow()
        {
            CurrentWindow.Dispatcher.InvokeAsync(() =>
            {
                ReceivePage.TextBox_Receive.Text = CurrentTabData.ToolBarSetting.ReceiveWindowText.ToString();
            });
        }

        //開始連線

        
    }
}
