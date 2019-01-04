using Contoso.CognitivePipeline.SharedModels.Cognitive;
using Contoso.CognitivePipeline.SharedModels.Models;
using Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Services
{
    public partial class ComputerVisionService
    {
        public static async Task<CustomVisionClassification> GetCustomClassification(HttpClient httpClient, byte[] inputImage)
        {
            string result = "";
            // Add Microsoft Azure Cognitive Service Token to HttpClient header
            var key = GlobalSettings.GetKeyValue("CognitiveServices-ShelfCompliance-Key");
            var baseUrl = GlobalSettings.GetKeyValue("CognitiveServices-ShelfCompliance-Endpoint");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Prediction-Key", key);
            
            var url = $"{baseUrl}";

            using (ByteArrayContent content = new ByteArrayContent(inputImage))
            {
                // Send request
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await httpClient.PostAsync(url, content);
                
                result = await response.Content.ReadAsStringAsync();
            }

            var parsedResult = JsonConvert.DeserializeObject<CustomVisionClassification>(result);
            parsedResult.IsSuccessful = true;
            parsedResult.CognitiveName = InstructionFlag.ShelfCompliance.ToString();

            return parsedResult;
        }
    }
}
