using Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Services
{
    public class ComputerVisionSdk
    {
        private const bool DetectOrientation = true;

        public ApiKeyServiceClientCredentials Credentials { get; set; }
        public ILogger Log { get; }
        public string Endpoint { get; set; }

        public ComputerVisionSdk(ILogger log)
        {
            Endpoint = GlobalSettings.GetKeyValue("CognitiveServices-CompVision-Endpoint");
            Credentials = new ApiKeyServiceClientCredentials(GlobalSettings.GetKeyValue("CognitiveServices-CompVision-Key"));
            Log = log;
        }

        /// <summary>
        /// Uploads the image to Cognitive Services and performs OCR.
        /// </summary>
        /// <param name="inputImage">The image byte[] to be analized.</param>
        /// <param name="language">The language code to recognize. Use Unk for automatic detection</param>
        /// <returns>Awaitable OCR result.</returns>
        public async Task<OcrResult> RecognizeImageOCRAsync(byte[] inputImage, OcrLanguages language)
        {
            // Create Cognitive Services Vision API Service client.
            using (var client = new ComputerVisionClient(Credentials) { Endpoint = Endpoint })
            {
                Log.LogInformation("ComputerVisionClient is created");

                // Upload an image and perform OCR.
                Log.LogInformation("Calling ComputerVisionClient.RecognizePrintedTextInStreamAsync()...");
                OcrResult ocrResult = await client.RecognizePrintedTextInStreamAsync(!DetectOrientation, new MemoryStream(inputImage), language);
                return ocrResult;
            }
        }

        public async Task<ImageAnalysis> AnalyzeImageAsync(byte[] inputImage, OcrLanguages language)
        {
            // Create Cognitive Services Vision API Service client.
            using (var client = new ComputerVisionClient(Credentials) { Endpoint = Endpoint })
            {
                Log.LogInformation("ComputerVisionClient is created");

                // Upload an image and perform OCR.
                Log.LogInformation("Calling ComputerVisionClient.RecognizePrintedTextInStreamAsync()...");
                ImageAnalysis cognitiveResult = await client.AnalyzeImageInStreamAsync(new MemoryStream(inputImage));
                return cognitiveResult;
            }
        }
    }
}