using Polly;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.ClientSDK.Base
{
    /// <summary>
    /// TODO: Leverage this base class to implement retry policy
    /// </summary>
    public abstract class BaseCognitiveApi
    {
        protected string apiKey;
        protected string apiBaseUrl;
        protected const int MaximumRetries = 3;
        
        /// <summary>
        /// Initialize new client and set the baseUrl and key to be used to make strongly typed API calls.
        /// </summary>
        /// <param name="key">API subscription key from Azure API Management Service</param>
        /// <param name="baseUrl">API base url (https://your-api-management-name.azure-api.net)</param>
        public BaseCognitiveApi(string key, string baseUrl)
        {
            apiKey = key;
            apiBaseUrl = baseUrl;
        }

        //TODO: Implement retry policy
        //Using Polly to build a retry resiliency as part of the API execution :)
        //protected Policy Policy;
        //protected BaseCognitiveApi()
        //{
        //    // Define Retry Policy
        //    Policy = Policy
        //        .Handle<ApiException>()
        //        .Or<HttpRequestException>()
        //        .Or<TimeoutException>()
        //        .RetryAsync(MaximumRetries, async (exception, retry) =>
        //        {
        //            // Handle Unauthorized in case Authentication service is implemented.
        //            if (exception is ApiException apiException && apiException.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                if (retry == 1)
        //                {
        //                    // The first time an Unauthorized exception occurs, 
        //                    // try to acquire an Access Token silently and try again
        //                    //TODO: API authentication token renewal logic
        //                }
        //                else if (retry == 2)
        //                {
        //                    // If refreshing the access token silently failed, show the login UI 
        //                    // and let the user enter his credentials again
        //                    //TODO: API resigning logic
        //                }
        //            }
        //            // Handle everything else
        //            else
        //            {
        //                // Wait a bit and try again
        //                Thread.Sleep(TimeSpan.FromSeconds(retry));
        //            }
        //        });
        //}
    }
}
