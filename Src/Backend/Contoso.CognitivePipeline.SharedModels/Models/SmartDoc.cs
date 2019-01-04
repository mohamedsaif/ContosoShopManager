using Contoso.CognitivePipeline.SharedModels.Cognitive;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    public class SmartDoc : BaseModel
    {
        public string OwnerId { get; set; }
        public string DocName { get; set; }
        public string DocUrl { get; set; }
        public string TileSizeUrl { get; set; }
        public string IconSizeUrl { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ClassificationType DocType { get; set; }
        public string PrimaryClassification { get; set; }
        public double PrimaryClassificationConfidence { get; set; }
        public List<ProcessingStep> CognitivePipelineActions { get; set; } = new List<ProcessingStep>();
        
        //Bytes will not stored in the db. Instead will remain in the blob storage and accessed via the DocUri
        //public byte[] DocBytes { get; set; }

        //public string PrimaryClassificationTagsRaw { get; set; }
        //public List<SmartDocTag> ClassificationTags { get; set; }
        //public string OCRTextTagsRaw { get; set; }
        //public List<SmartDocTag> OCRTextTags { get; set; }

        public string ErrorMessage { get; set; }
        public string Status { get; set; }
    }
}
