using Contoso.CognitivePipeline.ClientSDK.Abstractions;
using Contoso.CognitivePipeline.ClientSDK.Base;
using Contoso.CognitivePipeline.SharedModels.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.ClientSDK.Client
{
    /// <summary>
    /// Holds all APIs associated with this service
    /// </summary>
    public class ShelvesComplianceClient : BaseCognitiveApi
    {
        /// <summary>
        /// Initialize new client and set the baseUrl and key to be used to make strongly typed API calls.
        /// </summary>
        /// <param name="key">API subscription key from Azure API Management Service</param>
        /// <param name="baseUrl">API base url (https://your-api-management-name.azure-api.net)</param>
        public ShelvesComplianceClient(string key, string baseUrl) : base(key, baseUrl)
        {
        }

        /// <summary>
        /// Execute a http request and return the results
        /// </summary>
        /// <param name="ownerId">Document Owner Id (like EmployeeId or CustomerId)</param>
        /// <param name="isAsync">Flag to indicate if operations need to execute immediately or will be queued</param>
        /// <param name="doc">The binary of the document being processed</param>
        /// <param name="isMinimum">Flag to optimize the output by removing additions details from the results.</param>
        /// <returns>returns EmployeeId</returns>
        public async Task<ShelfCompliance> ValidateShelvesCompliace(string ownerId, byte[] doc, bool isAsync = false, bool isMinimum = true)
        {
            IShelvesComplianceAPI api = RestService.For<IShelvesComplianceAPI>(apiBaseUrl);
            var docStream = new StreamPart(new MemoryStream(doc), "doc.file");
            try
            {
                var result = await api.SubmitDoc(ownerId, docStream, apiKey, isAsync, isMinimum);
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
