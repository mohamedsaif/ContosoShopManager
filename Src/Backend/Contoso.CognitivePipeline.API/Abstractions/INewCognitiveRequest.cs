using Contoso.CognitivePipeline.SharedModels.Models;
using System.Threading.Tasks;

namespace Contoso.SB.API.Abstractions
{
    public interface INewCognitiveRequest<T>
    {
        Task<string> SendNewRequest(NewRequest<T> message, string id, bool isAsync);
    }
}