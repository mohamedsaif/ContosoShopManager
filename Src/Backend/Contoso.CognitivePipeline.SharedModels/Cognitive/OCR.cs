using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Cognitive
{
    public class OCR : CognitiveBase
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("textAngle")]
        public long TextAngle { get; set; }

        [JsonProperty("orientation")]
        public string Orientation { get; set; }

        [JsonProperty("regions")]
        public Region[] Regions { get; set; }
    }

    public class Region
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("lines")]
        public Line[] Lines { get; set; }
    }

    public class Line
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("words")]
        public Word[] Words { get; set; }
    }

    public class Word
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
