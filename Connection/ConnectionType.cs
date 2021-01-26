using System;
using System.Windows.Controls;

namespace EasyCom.Connection
{
    public class ConnectionType
    {
        public string Name { get; set; }
        public Page AdvanceSettingsPage { get; set; } //該連線的設定頁面
        public Type ConnectionObjectType { get; set; } //該連線的物件類型
        public IConnectionCommand CommandHandle { get; set; }

        public ConnectionType(string name, Page page, Type t,IConnectionCommand commandHandle)
        {
            this.Name = name;
            this.AdvanceSettingsPage = page;
            this.ConnectionObjectType = t;
            this.CommandHandle = commandHandle;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
