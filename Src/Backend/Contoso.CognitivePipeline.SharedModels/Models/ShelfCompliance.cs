using Contoso.CognitivePipeline.SharedModels.Cognitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    public class ShelfCompliance : SmartDoc
    {
        public bool IsCompliant { get; set; }
        public bool IsConfidenceAcceptable { get; set; }
        public double Confidence { get; set; }
        public string DetectionNotes { get; set; }
        public CustomVisionClassification Classification { get; set; }
    }
}
