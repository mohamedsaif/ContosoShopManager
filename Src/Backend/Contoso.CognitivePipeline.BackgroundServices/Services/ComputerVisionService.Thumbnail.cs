using Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities;
using Contoso.CognitivePipeline.SharedModels.Cognitive;
using Contoso.CognitivePipeline.SharedModels.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Services
{
    public partial class ComputerVisionService
    {
        public static async Task<Thumbnail> GetThumbnailAsync(HttpClient httpClient, byte[] inputImage, Stream outputImage, int width, int height)
        {
            // Add Microsoft Azure Cognitive Service Token to HttpClient header
            var key = GlobalSettings.GetKeyValue("CognitiveServices-CompVision-Key");
            var baseUrl = GlobalSettings.GetEnvironmentVariable("CognitiveServices-CompVision-Endpoint");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            // Create Cognitive Service request url with parameters
            var serviceAPIUrl = $"generateThumbnail?width={width}&height={height}&smartCropping=true";
            
            var url = $"{baseUrl}/{serviceAPIUrl}";

            using (ByteArrayContent content = new ByteArrayContent(inputImage))
            {
                // Send request
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await httpClient.PostAsync(url, content);

                // Write cropped image to output stream
                var resizedImage = await response.Content.ReadAsStreamAsync();
                resizedImage.CopyTo(outputImage);
                return new Thumbnail
                {
                    CognitiveName = InstructionFlag.Thumbnail.ToString(),
                    IsSuccessful = true,
                    ThumbnailHeight = height,
                    ThumbnailWidth = width,
                    ThumbnailUrl = "NA"
                };
            }
        }
    }
}
