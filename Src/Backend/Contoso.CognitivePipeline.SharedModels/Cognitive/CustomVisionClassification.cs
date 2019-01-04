using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Cognitive
{
    public partial class CustomVisionClassification : CognitiveBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("iteration")]
        public string Iteration { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("predictions")]
        public Prediction[] Predictions { get; set; }

        [JsonProperty("isCompliant")]
        public bool IsCompliant { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class Prediction
    {
        [JsonProperty("probability")]
        public double Probability { get; set; }

        [JsonProperty("tagId")]
        public string TagId { get; set; }

        [JsonProperty("tagName")]
        public string TagName { get; set; }
    }
}
