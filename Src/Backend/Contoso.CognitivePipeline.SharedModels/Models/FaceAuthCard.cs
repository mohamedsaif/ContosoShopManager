using Contoso.CognitivePipeline.SharedModels.Cognitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    public class FaceAuthCard : SmartDoc
    {
        public bool IsAuthenticationSuccessful { get; set; }
        public string DetectedFaceName { get; set; }
        public string DetectedFaceOwnerId { get; set; }
        public string DetectionNotes { get; set; }
        public FaceAuth FaceDetails { get; set; }
    }
}
