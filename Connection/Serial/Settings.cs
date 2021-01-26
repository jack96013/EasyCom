using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCom.Connection.Serial
{
    public class Settings
    {
        public int ComPort { get; set; } = -1;
        public uint Baudrate { get; set; } = 0;
        public bool RTSEnable { get; set; } = false;
        public bool DTREnable { get; set; } = false;
        public BasicItem<ushort> DataBits { get; set; } = null;
        public BasicItem<StopBits> StopBits { get; set; } = null;
        public BasicItem<Parity> Parity { get; set; } = null;
        public BasicItem<Handshake> Handshake { get; set; } = null;

        public bool Equal(Settings settings)
        {

            if (ComPort != settings.ComPort)
                return false;
            if (Baudrate != settings.Baudrate)
                return false;
            if (RTSEnable != settings.RTSEnable)
                return false;
            if (DTREnable != settings.DTREnable)
                return false;
            if (DataBits.Value != settings.DataBits.Value)
                return false;
            if (StopBits.Value != settings.StopBits.Value)
                return false;
            if (Parity.Value != settings.Parity.Value)
                return false;
            if (Handshake.Value != settings.Handshake.Value)
                return false;
            return true;
        }
    }
}
