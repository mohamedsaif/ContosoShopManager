using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities
{
    
    public class FunctionExecuter
    {
        public static HttpClient httpClient = new HttpClient();
        private static string functionKey = GlobalSettings.GetKeyValue("FunctionGlobalKey");

        public static async Task<IActionResult> CallFunction(string functionUri, StringContent content)
        {
            var result = "";
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-functions-key", functionKey);

            using (HttpResponseMessage response = (await httpClient.PostAsync(functionUri, content)))
            using (HttpContent responseContent = response.Content)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = await responseContent.ReadAsStringAsync();
                }
                else
                {
                    //TODO: Handle the non 200 code response messages here
                    
                    return (ActionResult)new BadRequestObjectResult(result);
                }
            }

            return (ActionResult)new OkObjectResult(result);
        }
    }
}
