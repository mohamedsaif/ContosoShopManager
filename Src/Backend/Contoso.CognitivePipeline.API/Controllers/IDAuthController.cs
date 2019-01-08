using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contoso.CognitivePipeline.API.Helpers;
using Contoso.CognitivePipeline.SharedModels.Models;
using Contoso.SB.API.Abstractions;
using Contoso.SB.API.BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Contoso.CognitivePipeline.API.Controllers
{
    //Using the [controller] in route it will not be detected by each swagger documentation. So I'm using explicit route
    //[Route("api/[controller]")]
    [Route("api/idauth")]
    public class IDAuthController : ControllerBase
    {
        public const ClassificationType docType = ClassificationType.ID;
        IStorageRepository storageRepository;
        IDocumentDBRepository<SmartDoc> docRepository;
        IDocumentDBRepository<User> userRepository;
        INewCognitiveRequest<SmartDoc> newReqService;

        public IDAuthController(IStorageRepository storage, IDocumentDBRepository<SmartDoc> documentDBRepository, IDocumentDBRepository<User> uRepository, INewCognitiveRequest<SmartDoc> newAsyncReq)
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
        [ProducesResponseType(200)]
        public IActionResult Get()
        {
            return Ok("{\"status\": \"" + this.GetType().Name + " working...\"}");
        }

        /// <summary>
        /// Submit a new Document file to be processed by the Cognitive Pipeline
        /// </summary>
        /// <returns>The result of document after processing</returns>
        /// <param name="ownerId">Document Owner Id (like EmployeeId or CustomerId)</param>
        /// <param name="isAsync">Flag to indicate if operations need to execute immediately or will be queued</param>
        /// <param name="doc">The binary of the document being processed</param>
        /// <param name="isMinimum">Flag to optimize the output by removing additions details from the results.</param>
        [HttpPost("{ownerId}/{isAsync}/{isMinimum?}")]
        [ProducesResponseType(200, Type = typeof(EmployeeId))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SubmitDoc(string ownerId, bool isAsync, IFormFile doc, bool isMinimum = true)
        {
            NewRequest<SmartDoc> newReq = null;
            string result = null;
            try
            {
                newReq = await ClassificationRequestHelper.CreateNewRequest(
                    ownerId, isAsync, doc, docType, storageRepository, docRepository, userRepository);

                result = await newReqService.SendNewRequest(newReq, newReq.RequestItem.Id, isAsync);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            //Reduce the size of the payload if isMinimum = true
            if (isMinimum)
            {
                var output = JsonConvert.DeserializeObject<EmployeeId>(result);
                output.OptimizeSmartDoc(isMinimum);
                result = JsonConvert.SerializeObject(output);
            }

            return Ok(result);
        }
    }
}