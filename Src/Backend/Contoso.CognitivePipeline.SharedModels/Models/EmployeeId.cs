using Contoso.CognitivePipeline.SharedModels.Cognitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    public class EmployeeId : SmartDoc
    {
        public bool IsAuthenticationSuccessful { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeJobTitle { get; set; }
        public string EmployeeNum { get; set; }
        public string DetectionNotes { get; set; }
    }
}
