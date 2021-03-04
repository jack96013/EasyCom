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
        bool SendData(byte[] data,bool async=true);
        bool AllowApplySettingsWithoutClose { get; }
        bool ApplySettings();
        
    }
}
