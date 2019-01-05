using Contoso.SB.API.Abstractions;

//using Contoso.SB.API.Models;
using Contoso.SB.API.Settings;
using Contoso.CognitivePipeline.SharedModels.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.SB.API.Services
{
    public class NewCognitiveReqService<T> : INewCognitiveRequest<T>
    {
        private BackgroundServicesSettings settings;
        private IQueueRepository queue;

        public NewCognitiveReqService(IConfiguration config, IQueueRepository queueRepository)
        {
            settings = config.GetSection(typeof(BackgroundServicesSettings).Name).Get<BackgroundServicesSettings>();
            queue = queueRepository;
        }

        public async Task<string> SendNewRequest(NewRequest<T> message, string id, bool IsAsync)
        {
            // TODO: Refactor out the pure HTTP calls

            string result = null;
            //Obtain Azure Function Uri and Key from the settings
            var uri = settings.NewReqUri + $"/{id}";
            var key = settings.NewReqKey;

            //Prepare the message to send by converting it to JSON and add it to StringContent
            var jsonMessage = JsonConvert.SerializeObject(message);

            //Request has IsAsync which will be passed on to the function to act accordingly
            //If call is sync, then call the classification function directly
            //If call is async, queue the new request to trigger the async process

            var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-functions-key", key);
                using (HttpResponseMessage response = client.PostAsync(uri, content).Result)
                using (HttpContent responseContent = response.Content)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        result = await responseContent.ReadAsStringAsync();
                    }
                    else
                    {
                        //TODO: Handle the non-200 code response messages here
                    }
                }
            }

            return result;
        }
    }
}