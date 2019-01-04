using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.SB.API.Settings
{
    public class StorageSettings
    {
        //Azure storage settings
        public string StorageName { get; set; }
        public string StorageKey { get; set; }
        public string StorageContainer { get; set; }
        public string StorageNewRequestQueue { get; set; }
        public string StorageCallbackRequestQueue { get; set; }
    }
}
