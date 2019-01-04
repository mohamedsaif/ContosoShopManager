using System;

namespace Contoso.CognitivePipeline.SharedModels.Models
{
    public class ProcessingStep
    {
        public string StepName { get; set; }
        public string Status { get; set; }
        public string Output { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public bool IsSuccessful { get; set; }
    }
}