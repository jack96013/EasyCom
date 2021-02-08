using EasyCom.General;
using System.Text;
using System.Windows.Documents;

namespace EasyCom.Settings
{
    public class ToolBarSetting
    {

        public bool Connected { get ; set ; }
        public object ConnectionSettings { get ; set ; }
        public bool ReceiveAutoSpilt { get ; set ; }
        public bool ReceiveShowTime { get ; set ; }
        public MainWindowOption.LineEndingItem ReceiveLineEnding { get ; set ; }
        public uint ReceiveTimeOut { get ; set ; }
        public MainWindowOption.DecodingItem ReceiveDecodeType { get ; set ; }
        public MainWindowOption.LineEndingItem SendLineEnding { get ; set ; }
        public bool SendHex { get ; set ; }
        public bool SendShowOnReceive { get ; set ; }
        public bool SendAutoSenderEnable { get ; set; }
        public uint SendAutoSenderInterval { get ; set ; }
        public bool SendAutoSenderAmountEnable { get; set ; }
        public uint SendAutoSenderAmount { get; set; }
        public string SendText { get; set; }
        public string SendPath { get; set; }
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
