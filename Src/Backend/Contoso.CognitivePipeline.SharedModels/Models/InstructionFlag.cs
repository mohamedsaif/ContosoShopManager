using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstructionFlag
    {
        AnalyzeImage,
        AnalyzeText,
        Thumbnail,
        FaceAuthentication,
        ShelfCompliance
    }
}