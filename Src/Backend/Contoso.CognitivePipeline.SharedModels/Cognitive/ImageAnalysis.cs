using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Cognitive
{
    public class ImageAnalysis : CognitiveBase
    {
        [JsonProperty("categories")]
        public Category[] Categories { get; set; }

        [JsonProperty("adult")]
        public Adult Adult { get; set; }

        [JsonProperty("color")]
        public PrimaryColor Color { get; set; }

        [JsonProperty("tags")]
        public Tag[] Tags { get; set; }

        [JsonProperty("description")]
        public Description Description { get; set; }

        [JsonProperty("faces")]
        public Face[] Faces { get; set; }

        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Adult
    {
        [JsonProperty("isAdultContent")]
        public bool IsAdultContent { get; set; }

        [JsonProperty("isRacyContent")]
        public bool IsRacyContent { get; set; }

        [JsonProperty("adultScore")]
        public double AdultScore { get; set; }

        [JsonProperty("racyScore")]
        public double RacyScore { get; set; }
    }

    public class Category
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public class PrimaryColor
    {
        [JsonProperty("dominantColorForeground")]
        public string DominantColorForeground { get; set; }

        [JsonProperty("dominantColorBackground")]
        public string DominantColorBackground { get; set; }

        [JsonProperty("dominantColors")]
        public string[] DominantColors { get; set; }

        [JsonProperty("accentColor")]
        public string AccentColor { get; set; }

        [JsonProperty("isBwImg")]
        public bool ColorIsBwImg { get; set; }

        [JsonProperty("isBWImg")]
        public bool IsBwImg { get; set; }
    }

    public class Description
    {
        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("captions")]
        public Caption[] Captions { get; set; }
    }

    public class Caption
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public class Face
    {
        [JsonProperty("age")]
        public long Age { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("faceRectangle")]
        public FaceRectangle FaceRectangle { get; set; }
    }

    //No need for it as it is provided as part for FaceAuth types
    //public class FaceRectangle
    //{
    //    [JsonProperty("left")]
    //    public long Left { get; set; }

    //    [JsonProperty("top")]
    //    public long Top { get; set; }

    //    [JsonProperty("width")]
    //    public long Width { get; set; }

    //    [JsonProperty("height")]
    //    public long Height { get; set; }
    //}

    public class Metadata
    {
        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }

    public class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
