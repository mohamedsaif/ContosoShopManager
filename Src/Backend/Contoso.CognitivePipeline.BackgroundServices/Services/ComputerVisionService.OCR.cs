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
        public static async Task<OCR> GetOCRAsync(HttpClient httpClient, byte[] inputImage)
        {
            string result = "";
            // Add Microsoft Azure Cognitive Service Token to HttpClient header
            var key = GlobalSettings.GetKeyValue("CognitiveServices-CompVision-Key");
            var baseUrl = GlobalSettings.GetKeyValue("CognitiveServices-CompVision-Endpoint");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            // Create Cognitive Service request url with parameters
            var serviceAPIUrl = $"ocr?language=unk&detectOrientation=true";
            
            var url = $"{baseUrl}/{serviceAPIUrl}";

            using (ByteArrayContent content = new ByteArrayContent(inputImage))
            {
                // Send request
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await httpClient.PostAsync(url, content);
                
                result = await response.Content.ReadAsStringAsync();
            }

            var parsedResult = JsonConvert.DeserializeObject<OCR>(result);
            parsedResult.IsSuccessful = true;
            parsedResult.CognitiveName = InstructionFlag.AnalyzeText.ToString();

            return parsedResult;
        }
    }
}
