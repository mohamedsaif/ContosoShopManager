using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    public class NewRequest<T> : BaseModel
    {
        public string OwnerId { get; set; }
        public string ItemReferenceId { get; set; }
        public T RequestItem { get; set; }
        public List<string> Instructions { get; set; }
        public string Status { get; set; }
        public bool IsAsync { get; set; }
        public List<ProcessingStep> Steps { get; set; } = new List<ProcessingStep>();
    }

    public class FlagConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
        {
            var flags = existingValue.ToString();
            int arrayStart = flags.IndexOf('[');
            var parsedFlags = flags.Substring(arrayStart, flags.Length - arrayStart).Split(',');
            foreach(var flag in parsedFlags)
            {
                var cleanedFlag = flag.Replace(" ", "").Replace("\"", "");
                InstructionFlag parsedFlag = (InstructionFlag)Enum.Parse(typeof(InstructionFlag), cleanedFlag);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
        {
            var flags = value.ToString()
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => $"\"{f}\"");

            writer.WriteRawValue($"[{string.Join(", ", flags)}]");
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
