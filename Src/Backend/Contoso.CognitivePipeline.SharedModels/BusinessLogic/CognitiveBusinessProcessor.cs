using Contoso.CognitivePipeline.SharedModels.Cognitive;
using Contoso.CognitivePipeline.SharedModels.Mapper;
using Contoso.CognitivePipeline.SharedModels.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.BusinessLogic
{
    public class CognitiveBusinessProcessor
    {
        public static string GetOCRLine(Line line)
        {
            string result = "";
            foreach(var word in line.Words)
            {
                if (!string.IsNullOrEmpty(result))
                    result += " " + word.Text;
                else
                    result = word.Text;
            }

            return result;
        }

        public static EmployeeId ProcessEmployeeIdDocument(NewRequest<SmartDoc> newReq)
        {
            EmployeeId result = DocumentMapper.MapDocument<EmployeeId>(newReq.RequestItem);

            //In order to process document as Employee Id, we need a successful OCR
            var ocrStep = newReq.RequestItem.CognitivePipelineActions.Where(s => s.StepName == InstructionFlag.AnalyzeText.ToString()).FirstOrDefault();
            if(ocrStep != null)
            {
                var ocrResult = JsonConvert.DeserializeObject<OCR>(ocrStep.Output);

                //Employee Ids in general have a fixed layout, so it practical to assume that location of text will be identical for every id
                //OCR will return an array of lines in the 1st region: 1st is Name, 2nd is Title, 3rd is Employee Id (the rest of read data is not relevant)

                if (ocrResult.Regions.Length == 0)
                    result.DetectionNotes = "Invalid Employee Id";
                else
                {

                    var region = ocrResult.Regions[0];

                    if (region.Lines.Length >= 3)
                    {
                        result.EmployeeName = GetOCRLine(region.Lines[0]);
                        result.EmployeeJobTitle = GetOCRLine(region.Lines[1]);
                        result.EmployeeNum = GetOCRLine(region.Lines[2]);
                        result.PrimaryClassification = "EmployeeIdAuthentication";
                        result.PrimaryClassificationConfidence = 1;
                        result.DetectionNotes = "Valid Employee Id";
                    }
                    else
                        result.DetectionNotes = "Invalid Employee Id*";
                }
            }

            if (string.IsNullOrEmpty(result.DetectionNotes))
                result.DetectionNotes = "No OCR Found";

            return result;
        }

        public static FaceAuthCard ProcessFaceAuthentication(NewRequest<SmartDoc> newReq)
        {
            FaceAuthCard result = DocumentMapper.MapDocument<FaceAuthCard>(newReq.RequestItem);
            result.DetectionNotes = "Invalid Face Verification";

            var faceStep = newReq.RequestItem.CognitivePipelineActions.Where(s => s.StepName == InstructionFlag.FaceAuthentication.ToString()).FirstOrDefault();
            if (faceStep != null)
            {
                var faceResult = JsonConvert.DeserializeObject<FaceAuth>(faceStep.Output);

                result.DetectedFaceName = "";

                //TODO: Update Owner Id to be verified against other faces than Request/Document Owner
                result.DetectedFaceOwnerId = newReq.RequestItem.OwnerId;
                result.IsAuthenticationSuccessful = faceResult.IsIdentical;
                result.FaceDetails = faceResult;
                result.DetectionNotes = faceResult.IsIdentical ? "Successful Face Verification" : "Invalid Face Verification";

                return result;
            }

            return result;
        }

        public static ShelfCompliance ProcessShelfCompliance(NewRequest<SmartDoc> newReq, double threshold)
        {
            ShelfCompliance result = DocumentMapper.MapDocument<ShelfCompliance>(newReq.RequestItem);
            result.DetectionNotes = "Invalid Classification";

            var shelfComplianceStep = newReq.RequestItem.CognitivePipelineActions.Where(s => s.StepName == InstructionFlag.ShelfCompliance.ToString()).FirstOrDefault();
            if (shelfComplianceStep != null)
            {
                var classificationResult = JsonConvert.DeserializeObject<CustomVisionClassification>(shelfComplianceStep.Output);

                //Get the top classification for compliant and non-compliant tags 
                var topPrediction = classificationResult.Predictions.OrderByDescending(p => p.Probability).FirstOrDefault();
                if(topPrediction != null)
                {
                    if (topPrediction.TagName == "Compliant")
                        result.IsCompliant = true;

                    if (topPrediction.Probability >= threshold)
                    {
                        result.Confidence = topPrediction.Probability;
                        result.IsConfidenceAcceptable = true;
                        result.PrimaryClassification = InstructionFlag.ShelfCompliance.ToString();
                        result.PrimaryClassificationConfidence = topPrediction.Probability;
                        result.DetectionNotes = "Successful Classification";
                    }
                    else
                    {
                        //Unidentified or low quality picture
                        result.Confidence = -1;
                        result.IsConfidenceAcceptable = false;
                        result.PrimaryClassification = InstructionFlag.ShelfCompliance.ToString();
                        result.PrimaryClassificationConfidence = topPrediction.Probability;
                        result.DetectionNotes = $"Below ({threshold}) threshold Classification";
                    }
                }
            }

            return result;
        }

        public static SmartDoc ProcessThumbnail(NewRequest<SmartDoc> newReq)
        {
            var thumbnailStep = newReq.RequestItem.CognitivePipelineActions.Where(s => s.StepName == InstructionFlag.Thumbnail.ToString()).FirstOrDefault();
            if(thumbnailStep != null)
            {
                var thumbnailResult = JsonConvert.DeserializeObject<Thumbnail[]>(thumbnailStep.Output);
                newReq.RequestItem.TileSizeUrl = $"{newReq.RequestItem.DocUrl.Replace("smartdocs", "smartdocs-tile")}";
                newReq.RequestItem.IconSizeUrl = $"{newReq.RequestItem.DocUrl.Replace("smartdocs", "smartdocs-icon")}";
            }

            return newReq.RequestItem;
        }

        public static void ProcessImageAnalysis(NewRequest<SmartDoc> newReq)
        {

        }
    }
}
