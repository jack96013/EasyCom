using EasyCom.Connection;
using EasyCom.Connection.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCom.Settings
{
    public class MainWindowOption
    {
        private MainWindow mainWindow = null;
        private List<LineEndingItem> lineEndingTypeList = new List<LineEndingItem>();
        private List<DecodingItem> decodingTypeList = new List<DecodingItem>();
        //Properties
        public List<ConnectionType> ConnectionTypes { get; } = new List<ConnectionType>();
        public List<LineEndingItem> LineEndingTypeList { get => lineEndingTypeList; set => lineEndingTypeList = value; }
        public List<DecodingItem> DecodingTypeList { get => decodingTypeList; set => decodingTypeList = value; }
        //Default
        private int connectionTypesDefault = 0;
        private int lineEndingTypeDefault = 0;
        private int decodingTypeDefault = 0;
        public MainWindowOption(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            ConnectionTypeListInit();
            LineEndingTypeListInit();
            DecodingTypeListInit();
        }

        public void ConnectionTypeListInit()
        {
            Connection.Serial.PageSetting Connection_Setting_Serial_Page = new Connection.Serial.PageSetting(mainWindow);

            ConnectionTypes.Add(new ConnectionType("Serial", Connection_Setting_Serial_Page, typeof(SerialHelper), null));
            ConnectionTypes.Add(new ConnectionType("TCP", null, null,null));
            ConnectionTypes.Add(new ConnectionType("WebSocket", null, null, null)) ;
            ConnectionTypes.Add(new ConnectionType("Socket.IO", null, null,null));

            foreach (ConnectionType connectionType in ConnectionTypes)
            {
                IPageSetting page = (IPageSetting)connectionType.AdvanceSettingsPage;
                if (page != null)
                    page.SettingChangedCallBack = mainWindow.SettingChangedCallBack;

            }
        }

        public void LineEndingTypeListInit()
        {
            LineEndingTypeList.Add(new LineEndingItem("CR", "\r"));
            LineEndingTypeList.Add(new LineEndingItem("LF", "\n"));
            LineEndingTypeList.Add(new LineEndingItem("CRLF", "\r\n"));
        }

        public void DecodingTypeListInit()
        {
            DecodingTypeList.Add(new DecodingItem("ASCII", Encoding.ASCII));
            DecodingTypeList.Add(new DecodingItem("UTF8", Encoding.UTF8));
            DecodingTypeList.Add(new DecodingItem("HEX", null));
            
        }

        public ConnectionType getConnectionTypeByName(string name)
        {
            return ConnectionTypes.Find((s)=> { return s.Name == name; });
        }

        public class LineEndingItem
        {
            public string Name;
            public string Value;
            public LineEndingItem(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }
            public override string ToString()
            {
                return Name;
            }
        }

        public class DecodingItem
        {
            public string Name { get; set; }=null;
            public Encoding Value { get; set; } =null;
            public DecodingItem(string name, Encoding value)
            {
                this.Name = name;
                this.Value = value;
            }
            public override string ToString()
            {
                return Name;
            }
        }
    }
}
