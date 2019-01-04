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

namespace Contoso.CognitivePipeline.API.Controllers
{
    [Route("api/[controller]")]
    public class ShelvesComplianceController : ControllerBase
    {
        public const ClassificationType docType = ClassificationType.StoreShelf;
        IStorageRepository storageRepository;
        IDocumentDBRepository<SmartDoc> docRepository;
        IDocumentDBRepository<User> userRepository;
        INewCognitiveRequest<SmartDoc> newReqService;

        public ShelvesComplianceController(IStorageRepository storage, IDocumentDBRepository<SmartDoc> documentDBRepository, IDocumentDBRepository<User> uReposiroty, INewCognitiveRequest<SmartDoc> newAsyncReq)
        {
            storageRepository = storage;
            docRepository = documentDBRepository;
            userRepository = uReposiroty;
            newReqService = newAsyncReq;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("{\"status\": \"" + this.GetType().Name + " working...\"}");
        }

        /// <summary>
        /// Submit a new Docment file to be processed by the Cognitive Pipeline
        /// </summary>
        /// <returns>The result of document after processing</returns>
        /// <param name="ownerId">Document Owner Id (like EmployeeId or CustomerId)</param>
        /// <param name="isAsync">Indicate if the processing will be Sync or Async</param>
        /// <param name="doc">The acutal document binary data</param>
        [HttpPost("{ownerId}/{isAsync}")]
        [ProducesResponseType(200, Type = typeof(ShelfCompliance))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SubmitDoc(string ownerId, bool isAsync, IFormFile doc)
        {
            NewRequest<SmartDoc> newReq = null;
            string result = null;
            try
            {
                newReq = await ClassificationRequestHelper.CreateNewRequest(
                    ownerId, isAsync, doc, docType, storageRepository, docRepository, userRepository);

                result = await newReqService.SendNewReuqest(newReq, newReq.RequestItem.Id, isAsync);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            return Ok(result);
        }
    }
}