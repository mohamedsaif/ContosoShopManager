using Contoso.CognitivePipeline.SharedModels.Models;
using Contoso.SB.API.Abstractions;
using Contoso.SB.API.BusinessLogic;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.API.Helpers
{
    public class ClassificationRequestHelper
    {
        public static async Task<NewRequest<SmartDoc>> CreateNewRequest(
            string ownerId, 
            bool isAsync,
            IFormFile doc, 
            ClassificationType docType,
            IStorageRepository storageRepository,
            IDocumentDBRepository<SmartDoc> docRepository,
            IDocumentDBRepository<User> userRepository)
        {
            if (doc == null || doc.Length == 0)
                throw new InvalidOperationException("No file was selected");

            var owner = await userRepository.GetItemAsync(ownerId);

            if (owner == null)
                throw new InvalidOperationException("Invalid request");

            long size = doc.Length;

            // To hold the Url for the uploaded document
            string docName = "NA";
            string docUri = null;

            //Upload the submitted document to Azure Storage
            if (size > 0)
            {
                using (var stream = doc.OpenReadStream())
                {
                    var docExtention = doc.FileName.Substring(doc.FileName.LastIndexOf('.'));
                    var docId = Guid.NewGuid();
                    docName = $"{docType}-{docId}{docExtention}";
                    docUri = await storageRepository.CreateFile(docName, stream);
                }
            }


            // process uploaded files by creating a new SmartDoc record with the details and save it to CosmosDB
            var newDoc = new SmartDoc
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                DocType = docType,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                DocName = docName,
                DocUrl = docUri,
                Status = SmartDocStatus.Created.ToString(),
                Origin = docRepository.Origin
            };

            await docRepository.CreateItemAsync(newDoc);

            //Prepare the new request that will hold the document along with the processing instructions by the Cognitive Pipeline
            var newReq = new NewRequest<SmartDoc>
            {
                CreatedAt = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                IsDeleted = false,
                Instructions = DocumentInstructionsProcessor.GetInstructions(docType),
                ItemReferenceId = newDoc.Id,
                RequestItem = newDoc,
                Status = SmartDocStatus.Created.ToString(),
                Origin = docRepository.Origin,
                IsAsync = isAsync
            };

            return newReq;
        }
    }
}
