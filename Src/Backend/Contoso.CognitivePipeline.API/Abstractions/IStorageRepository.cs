using System.IO;
using System.Threading.Tasks;

namespace Contoso.SB.API.Abstractions
{
    public interface IStorageRepository
    {
        Task<string> CreateFile(string name, Stream stream);
    }
}