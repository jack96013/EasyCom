using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCom
{
    interface IPageSetting
    {
        Object currentConnection { get; set; }
        void BeSelected();
        Type settingsStructType { get; }
        void SettingsRestore(object settings);
        void GetSetting(object settings);
        void SetSettingDefault (object settings);
        Action<bool> SettingChangedCallBack { get; set; }
    }
}
