﻿using EasyCom.Connection;
using EasyCom.General;
using System.Text;
using System.Windows.Documents;

namespace EasyCom.Settings
{
    public class ToolBarSetting
    {

        public bool Connected { get ; set ; }
        //public IConnectionSettings ConnectionSettings { get ; set ; }
        public IConnectionSettings ConnectionSettings { get; set; }
        public bool ConnectionSettingsIsChanged { get; set; }
        public bool ReceiveAutoSpilt { get ; set ; }
        public bool ReceiveShowTime { get ; set ; }
        public MainWindowOption.LineEndingItem ReceiveLineEnding { get ; set ; }
        public uint ReceiveTimeOut { get ; set ; }
        public MainWindowOption.EncodingItem ReceiveEncodingType { get ; set ; }
        public MainWindowOption.LineEndingItem SendLineEnding { get ; set ; }
        public bool SendHex { get ; set ; }
        public MainWindowOption.EncodingItem SendEncodingType { get; set; }
        public bool SendShowOnReceive { get ; set ; }
        public bool SendAutoSenderEnable { get ; set; }
        public uint SendAutoSenderInterval { get ; set ; }
        public bool SendAutoSenderAmountEnable { get; set ; }
        public uint SendAutoSenderAmount { get; set; }
        public uint SendAutoSenderCurrentAmount { get; set; }
        public string SendText { get; set; }
        public string SendPath { get; set; }
        public int SendFileBufferSize { get; set; }
        public int SendFileInterval { get; set; }
        public bool ReceiveWindowTextUpdated { get; set; }
        public bool ReceiveWindowFreeze { get; set; }
        public StringBuilder ReceiveWindowText { get; set; }
        public FlowDocument FlowDocument { get ; set ; }
        public PopupDialog PopupDialogReceive { get; set; }
        public ToolBarSetting ShallowCopy()
        {
            return (ToolBarSetting)this.MemberwiseClone();
        }
    }
}
