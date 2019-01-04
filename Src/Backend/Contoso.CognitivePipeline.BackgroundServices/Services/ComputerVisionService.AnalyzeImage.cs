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
        public static async Task<ImageAnalysis> AnalyzeImage(HttpClient httpClient, byte[] inputImage)
        {
            string result = "";
            // Add Microsoft Azure Cognitive Service Token to HttpClient header
            var key = GlobalSettings.GetKeyValue("CognitiveServices-CompVision-Key");
            var baseUrl = GlobalSettings.GetKeyValue("CognitiveServices-CompVision-Endpoint");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            // Create Cognitive Service request url with parameters
            //Currently supported languages are:
            //en - English, Default.
            //ja - Japanese.
            //pt - Portuguese.
            //zh - Simplified Chinese.
            var serviceAPIUrl = $"analyze?visualFeatures=Categories,Tags,Faces,Description,Color,Adult&language=en";

            var url = $"{baseUrl}/{serviceAPIUrl}";

            using (ByteArrayContent content = new ByteArrayContent(inputImage))
            {
                // Send request
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await httpClient.PostAsync(url, content);
                
                result = await response.Content.ReadAsStringAsync();
            }

            var parsedResult = JsonConvert.DeserializeObject<ImageAnalysis>(result);
            parsedResult.IsSuccessful = true;
            parsedResult.CognitiveName = InstructionFlag.AnalyzeImage.ToString();

            return parsedResult;
        }
    }
}
