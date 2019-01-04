using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Cognitive
{
    public class CognitiveBase
    {
        public string CognitiveName { get; set; }
        public string Output { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
