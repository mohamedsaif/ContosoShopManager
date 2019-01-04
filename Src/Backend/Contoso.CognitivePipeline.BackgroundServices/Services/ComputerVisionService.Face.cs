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
        public static async Task<FaceAuth> GetFaceAuthAsync(HttpClient httpClient, byte[] inputImage, string personId)
        {
            //3 Operations needed to Authenticate a face agains a specific person
            //1st detect the face in provided image and take note of the faceId
            //2nd get the person in question PersonId in a givin PersonGroup
            //3rd Verify the detected face agains the retrieved person

            string faceDetectionResult = "";
            // Add Microsoft Azure Cognitive Service Token to HttpClient header
            var key = GlobalSettings.GetKeyValue("CognitiveServices-Face-Key");
            var baseUri = GlobalSettings.GetKeyValue("CognitiveServices-Face-Endpoint");
            var personGroupId = GlobalSettings.GetKeyValue("CognitiveServices-Face-PersonGroupId");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            // Create Cognitive Service request urls with parameters
            var faceDetectionService = $"detect??returnFaceId&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            var faceGetPersonIdService = $"persongroups/{personGroupId}/persons/{personId}";
            var faceVerificationService = $"verify";

            var faceDetectionUrl = $"{baseUri}/{faceDetectionService}";
            var faceGetPersonIdUrl = $"{baseUri}/{faceGetPersonIdService}";
            var faceVerificationUrl = $"{baseUri}/{faceVerificationService}";

            //Image bytes only needed during the face detection step
            using (ByteArrayContent content = new ByteArrayContent(inputImage))
            {
                // Send request to detect faces first
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await httpClient.PostAsync(faceDetectionUrl, content);
                
                faceDetectionResult = await response.Content.ReadAsStringAsync();
            }

            var parsedResult = JsonConvert.DeserializeObject<FaceAuth[]>(faceDetectionResult);

            FaceAuth result = null;

            if(parsedResult.Length == 0)
            {
                //No faces detected. Abort
                result = new FaceAuth { IsSuccessful = false, CognitiveName = "FaceAuth", Output = "No faces detected" };
            }
            else if(parsedResult.Length == 1)
            {
                //Exactly 1 face detected
                var detectedFace = parsedResult[0];

                //Retrieve specific person to verify that this face belong to him/her
                var personResponse = await httpClient.GetAsync(faceGetPersonIdUrl);
                var getPersonResult = await personResponse.Content.ReadAsStringAsync();
                var person = JsonConvert.DeserializeObject<Person>(getPersonResult);

                var faceVerifyInput = new FaceVerifyInput
                {
                    FaceId = detectedFace.FaceId,
                    PersonGroupId = personGroupId,
                    PersonId = person.PersonId
                };
                var verifyFaceContent = new StringContent(JsonConvert.SerializeObject(faceVerifyInput), Encoding.UTF8, "application/json");
                var verificationResponse = await httpClient.PostAsync(faceVerificationUrl, verifyFaceContent);
                var verificationResult = await verificationResponse.Content.ReadAsStringAsync();
                var faceVerificationResult = JsonConvert.DeserializeObject<FaceVerificationResult>(verificationResult);

                detectedFace.CognitiveName = InstructionFlag.FaceAuthentication.ToString();
                detectedFace.IsSuccessful = true;
                detectedFace.IsIdentical = faceVerificationResult.IsIdentical;
                detectedFace.Confidence = faceVerificationResult.Confidence;
                detectedFace.PersonId = person.PersonId;

                detectedFace.Output = JsonConvert.SerializeObject(detectedFace);
                result = detectedFace;
            }
            else
            {
                //More than 1 face detected. Abort as this can't be used for authentication
                result = new FaceAuth { IsSuccessful = false, CognitiveName = InstructionFlag.FaceAuthentication.ToString(), Output = "Too many faces detected" };
            }

            return result;
        }
    }
}
