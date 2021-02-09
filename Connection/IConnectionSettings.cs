using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCom.Connection
{
    public interface IConnectionSettings
    {
        void Parse(string key, string value, string report);
    }
}
