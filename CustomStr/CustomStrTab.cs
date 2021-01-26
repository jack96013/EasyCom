using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCom.CustomStr
{
    public class CustomStrTab
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<CustomStrData> StrList { get;} = new List<CustomStrData>();
        public CustomStrTab(string name)
        {
            this.Name = name;
        }
    }
}
