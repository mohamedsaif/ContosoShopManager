using Contoso.CognitivePipeline.BackgroundServices.Data;
using Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities;
using Contoso.CognitivePipeline.SharedModels.BusinessLogic;
using Contoso.CognitivePipeline.SharedModels.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Services
{
    public class CognitivePipelineResultProcessor
    {
        public static async Task<string> ProcessFinalResult(NewRequest<SmartDoc> processedReq, CosmosDBRepository<SmartDoc> smartDocsClient, CosmosDBRepository<User> usersClient)
        {
            string result = null;

            if (processedReq.RequestItem.DocType == ClassificationType.ID)
            {
                var tempResult = CognitiveBusinessProcessor.ProcessEmployeeIdDocument(processedReq);
                var isValid = await IsEmployeeExists(tempResult.EmployeeNum, usersClient);
                tempResult.IsAuthenticationSuccessful = isValid;
                tempResult.DetectionNotes = isValid ? "Authentication Sucessful" : "Authenticaton Failed";
                tempResult.PrimaryClassification = InstructionFlag.AnalyzeText.ToString();
                tempResult.PrimaryClassificationConfidence = 1;
                result = JsonConvert.SerializeObject(tempResult);

                //TODO: Future implementation to include face verfication on the employee scanned id as well
            }
            else if(processedReq.RequestItem.DocType == ClassificationType.Face)
            {
                //This is when a user is using face authentication service
                var tempResult = CognitiveBusinessProcessor.ProcessFaceAuthentication(processedReq);
                if (tempResult.IsAuthenticationSuccessful)
                {
                    var user = await GetEmployeeById(tempResult.DetectedFaceOwnerId, usersClient);
                    if (user != null)
                        tempResult.DetectedFaceName = user.DisplayName;
                }
                tempResult.PrimaryClassification = InstructionFlag.FaceAuthentication.ToString();
                tempResult.PrimaryClassificationConfidence = tempResult.FaceDetails.Confidence;
                result = JsonConvert.SerializeObject(tempResult);

            }
            else if(processedReq.RequestItem.DocType == ClassificationType.StoreShelf)
            {
                var threshold = double.Parse(GlobalSettings.GetKeyValue("CognitiveServices-ShelfCompliance-Threshold"));
                var tempResult = CognitiveBusinessProcessor.ProcessShelfCompliance(processedReq, threshold);
                processedReq.RequestItem.PrimaryClassification = InstructionFlag.ShelfCompliance.ToString();
                processedReq.RequestItem.PrimaryClassificationConfidence = tempResult.Confidence;
                result = JsonConvert.SerializeObject(tempResult);
            }
            else
            {
                if (processedReq.RequestItem.CognitivePipelineActions.Count > 0)
                {
                    var step = processedReq.RequestItem.CognitivePipelineActions[0];
                    processedReq.RequestItem.PrimaryClassification = step.StepName;
                    processedReq.RequestItem.PrimaryClassificationConfidence = 1;
                }
                result = JsonConvert.SerializeObject(processedReq);
            }

            await UpdateDocument(processedReq, smartDocsClient);

            return result;
        }

        public static async Task UpdateDocument(NewRequest<SmartDoc> newDocReq, CosmosDBRepository<SmartDoc> client)
        {
            foreach (var step in newDocReq.Steps)
            {
                if (newDocReq.RequestItem.CognitivePipelineActions == null)
                    newDocReq.RequestItem.CognitivePipelineActions = new List<ProcessingStep>();

                newDocReq.RequestItem.CognitivePipelineActions.Add(step);
            }
            await client.UpdateItemAsync(newDocReq.ItemReferenceId, newDocReq.RequestItem);
        }

        public static async Task<bool> IsEmployeeExists(string empNum, CosmosDBRepository<User> client)
        {
            var results = await client.GetItemsAsync((u) => u.AccountId == empNum && u.AccountType == "Employee" && u.IsActive);
            if (results.Count() > 0)
                return true;
            return false;
        }

        public static async Task<User> GetEmployeeById(string empId, CosmosDBRepository<User> client)
        {
            var results = await client.GetItemsAsync((u) => u.Id == empId && u.AccountType == "Employee" && u.IsActive);
            if (results.Count() > 0)
                return results.ElementAt(0);
            return null;
        }
    }
}
