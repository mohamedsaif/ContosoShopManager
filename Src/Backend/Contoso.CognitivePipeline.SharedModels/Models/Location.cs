﻿using Microsoft.Azure.Documents.Spatial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    public class Location : BaseModel
    {
        [JsonProperty("firstLineAddress")]
        public string FirstLineAddress { get; set; }

        [JsonProperty("secondLineAddress")]
        public string SecondLineAddress { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("point")]
        public Point Point { get; set; }
    }

}
