using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using EasyCom.General;

namespace EasyCom.Connection.Serial
{
    public class Options
    {
        public uint[] DefaultBaudRate = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200, 230400, 460800, 921600 };
        public List<object> BaudRateList { get; } = new List<object>();
        public List<ComPortItem> ComPortList { get; } = new List<ComPortItem>();
        public OptionsList<ushort> DataBitsList { get; } = new OptionsList<ushort>();
        public OptionsList<System.IO.Ports.StopBits> StopBitsList { get; } = new OptionsList<System.IO.Ports.StopBits>();
        public OptionsList<System.IO.Ports.Parity> ParityList { get; } = new OptionsList<System.IO.Ports.Parity>();
        public OptionsList<System.IO.Ports.Handshake> HandshakeList { get; } = new OptionsList<System.IO.Ports.Handshake>();

        public Options()
        {
            OptionsInitial();
        }

        private void OptionsInitial()
        {
            //Append Default Baudrates
            foreach (uint i in DefaultBaudRate)
            {
                BaudRateList.Add(i);
            }

            DataBitsList.Add("8", 8);
            DataBitsList.Add("7", 7);
            DataBitsList.Add("6", 6);
            DataBitsList.Add("5", 5);


            StopBitsList.Add("n", System.IO.Ports.StopBits.None, "無");
            StopBitsList.Add("1", System.IO.Ports.StopBits.One);
            StopBitsList.Add("1.5", System.IO.Ports.StopBits.OnePointFive);
            StopBitsList.Add("2", System.IO.Ports.StopBits.Two);

            ParityList.Add("n", System.IO.Ports.Parity.None, "無");
            ParityList.Add("o", System.IO.Ports.Parity.Odd, "奇數");
            ParityList.Add("e", System.IO.Ports.Parity.Even, "偶數");
            ParityList.Add("m", System.IO.Ports.Parity.Mark, "Mark (1)");
            ParityList.Add("s", System.IO.Ports.Parity.Space, "Space (0)");

            HandshakeList.Add("N", System.IO.Ports.Handshake.None, "無");
            HandshakeList.Add("R", System.IO.Ports.Handshake.RequestToSend, "硬體");
            HandshakeList.Add("X", System.IO.Ports.Handshake.XOnXOff, "軟體");
            HandshakeList.Add("D", System.IO.Ports.Handshake.RequestToSendXOnXOff, "兩者");

        }
    }
    public class ComPortItem
    {
        public int ComID { get; set; } = -1;
        public string Description { get; set; } = null;
        public bool Used { get; set; } = false;
        public ConnectionTabData UsedTab { get; set; }
        public bool Removed { get; set; } = false;

        public ComPortItem(int comName, string description)
        {
            this.ComID = comName;
            this.Description = description;
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("COM");
            stringBuilder.Append(ComID);
            if (Removed)
            {
                stringBuilder.Append(" (");
                stringBuilder.Append("已移除");
                stringBuilder.Append(")");
            }
            else
            {
                if (Description != null)
                {
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(Description);
                }

                if (Used)
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append(UsedTab.TabItem.Title);
                    stringBuilder.Append(" 占用");
                    stringBuilder.Append(")");
                }
            }
            return stringBuilder.ToString();

        }
    }
    public class BasicItem<TValue>
    {
        public string Name { get; set; } = null;
        public TValue Value { get; set; } = default(TValue);
        public string Description { get; set; } = null;


        public BasicItem(string name, TValue value, string description)
        {
            this.Name = name;
            this.Value = value;
            this.Description = description;
        }
        public BasicItem(string name, TValue value)
        {
            this.Name = name;
            this.Value = value;
            this.Description = null;
        }

        public string Display
        {
            get
            {
                if (Description is null)
                {
                    return Name;
                } 
                return Description;
            }
        }
    }

    public class OptionsList<ValueT>
    {
        public List<BasicItem<ValueT>> List { get; } = new List<BasicItem<ValueT>>();

        public void Add(string name, ValueT value, string description)
        {
            List.Add(new BasicItem<ValueT>(name, value, description));
        }

        public void Add(string name, ValueT value)
        {
            Add(name, value, null);
        }

        public BasicItem<ValueT> getOptionByName(string name)
        {
            return List.Find((s) => { return s.Name == name; });
        }
    }


    public class CustomBaudrate
    {
        public uint value = 0;
        private PopupDialogHost PopupDialogHost = null;
        private PageSetting page_Setting_Serial;
        public CustomBaudrate(PageSetting pageSettingSerial, PopupDialogHost popupDialogHost)
        {
            this.PopupDialogHost = popupDialogHost;
            this.page_Setting_Serial = pageSettingSerial;
            Label label = new Label() { Content = "" };

        }

        public override string ToString()
        {
            if (value == 0)
                return "自訂";
            else
                return String.Format("自訂 ( {0} )", value);
        }
        public void showDialog()
        {
            PageBaudrateCustomDialog dialog_BaudrateCustom = new PageBaudrateCustomDialog();
            if (value != 0)
                dialog_BaudrateCustom.BaudrateToShow = value;
            dialog_BaudrateCustom.onAccept = DialogAccept;
            dialog_BaudrateCustom.onCancel = DialogCancel;
            PopupDialogHost.CurrentDialog = new PopupDialog(dialog_BaudrateCustom);
            PopupDialogHost.Show();
            
        }

        public void DialogAccept(uint baud)
        {
            this.value = baud;
            PopupDialogHost.Close();
            page_Setting_Serial.ComboBox_BaudRate_Refresh();
            page_Setting_Serial.ComboBox_Baudrate.SelectedIndex = page_Setting_Serial.ComboBox_Baudrate.Items.Count - 1;

        }
        public void DialogCancel()
        {
            PopupDialogHost.Close();
        }
    }
}
