using Contoso.CognitivePipeline.SharedModels.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.ClientSDK.Abstractions
{
    public interface IIDAuthAPI
    {
        /// <summary>
        /// Service Interface to be used to generate request throw Refit
        /// </summary>
        /// <param name="ownerId">Document Owner Id (like EmployeeId or CustomerId)</param>
        /// <param name="doc">The binary of the document being processed</param>
        /// <param name="apiManagementKey">API subscription key provided by Azure API Management Service</param>
        /// <param name="isAsync">Flag to indicate if operations need to execute immediately or will be queued</param>
        /// <param name="isMinimum">Flag to optimize the output by removing additions details from the results.</param>
        /// <returns>returns EmployeeId</returns>
        [Multipart]
        [Post("/id/api/idauth/{ownerId}")]
        Task<EmployeeId> SubmitDoc(string ownerId, [AliasAs("doc")] StreamPart doc, [Header("Ocp-Apim-Subscription-Key")] string apiManagementKey, [Header("isAsync")] bool isAsync = false, [Header("isMinimum")] bool isMinimum = true);
    }
}
