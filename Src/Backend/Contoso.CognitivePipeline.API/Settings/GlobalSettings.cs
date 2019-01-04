using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.SB.API.Settings
{
    public class GlobalSettings
    {
        public string OriginName { get; set; } = "Contoso.SmartBank.API.V1";
        public string SystemVersion { get; set; } = "1.0.0";
        public string UpdatedAt { get; set; } = "1-JUL-2018";
    }
}
