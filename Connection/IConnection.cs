using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCom
{
    public interface IConnection
    {
        bool Connected { get; }
        void Open();
        void Close();
        void SendData(byte[] data);
        bool AllowApplySettingsWithoutClose { get; }
        bool ApplySettings();
        
    }
}
