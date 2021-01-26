using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCom.CustomStr
{
    public class CustomStrData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Text { get; set; } = "";
        public CustomStrData(string name, string description, string text)
        {
            this.Name = name;
            this.Description = description;
            this.Text = text;
        }
    }
}
