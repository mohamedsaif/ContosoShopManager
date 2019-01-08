using Contoso.SB.API.Abstractions;
using Contoso.SB.API.BusinessLogic;
using Contoso.CognitivePipeline.SharedModels.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Contoso.SB.API.Controllers
{
    [Route("api/classification")]
    public class ClassificationController : Controller
    {
        IStorageRepository storageRepository;
        IDocumentDBRepository<SmartDoc> docRepository;
        IDocumentDBRepository<User> userRepository;
        INewCognitiveRequest<SmartDoc> newReqService;

        public ClassificationController(IStorageRepository storage, IDocumentDBRepository<SmartDoc> documentDBRepository, IDocumentDBRepository<User> uRepository, INewCognitiveRequest<SmartDoc> newAsyncReq)
        {
            storageRepository = storage;
            docRepository = documentDBRepository;
            userRepository = uRepository;
            newReqService = newAsyncReq;
        }

        /// <summary>
        /// Check the health of the service
        /// </summary>
        /// <returns>The status message</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("{\"status\": \"Classification working...\"}");
        }

        /// <summary>
        /// Uploads a document to Azure storage
        /// </summary>
        /// <returns>The result of the uploaded document</returns>
        /// <param name="ownerId">Document owner Id</param>
        /// <param name="docType">One of the following ID, StoreShelf, Face, Generic, Unidentified which will determine the cognitive operations to be executed</param>
        /// <param name="isAsync">Flag to indicate if operations need to execute immediately or will be queued</param>
        /// <param name="doc">The binary of the document being processed</param>
        [HttpPost("{ownerId}/{docType}/{isAsync}")]
        public async Task<IActionResult> SubmitDoc(string ownerId, string docType, bool isAsync, IFormFile doc)
        {
            // TODO: Introduce another parameter to identify the needed classification services (like classification, OCR,...)

            if (doc == null || doc.Length == 0)
                return BadRequest("file not selected");

            var owner = await userRepository.GetItemAsync(ownerId);

            if (owner == null)
                return BadRequest("Invalid request");

            var proposedDocType = ClassificationType.Unidentified;
            var isValidType = Enum.TryParse<ClassificationType>(docType, out proposedDocType);
            if (!isValidType)
                return BadRequest("Invalid document type");

            long size = doc.Length;

            // full path to file in temp location
            string docName = "NA";
            string docUri = null;

            if (size > 0)
            {
                using (var stream = doc.OpenReadStream())
                {
                    var docExtention = doc.FileName.Substring(doc.FileName.LastIndexOf('.'));
                    var docId = Guid.NewGuid();
                    docName = $"{proposedDocType}-{docId}{docExtention}";
                    docUri = await storageRepository.CreateFile(docName, stream);
                }
                    
                //Sample to upload to local temp storage
                //var tempFolder = Path.GetTempPath();
                //var fileName = Path.GetRandomFileName();
                //var filePath = Path.Combine(tempFolder, fileName);
                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    await doc.CopyToAsync(stream);
                //}
            }


            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            var newDoc = new SmartDoc
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                DocType = proposedDocType,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                DocName = docName,
                DocUrl = docUri,
                Status = SmartDocStatus.Created.ToString(),
                Origin = docRepository.Origin
            };

            await docRepository.CreateItemAsync(newDoc);

            //Call NewReq function background service to start processing the new doc
            var newReq = new NewRequest<SmartDoc>
            {
                CreatedAt = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                IsDeleted = false,
                Instructions = DocumentInstructionsProcessor.GetInstructions(proposedDocType),
                ItemReferenceId = newDoc.Id,
                RequestItem = newDoc,
                Status = SmartDocStatus.Created.ToString(),
                Origin = docRepository.Origin,
                IsAsync = isAsync
            };

            var result = await newReqService.SendNewRequest(newReq, newDoc.Id, isAsync);

            return Ok(result);
        }
    }
}