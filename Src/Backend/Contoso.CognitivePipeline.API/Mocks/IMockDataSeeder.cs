using System.Threading.Tasks;

namespace Contoso.SB.API.Mocks
{
    public interface IMockDataSeeder
    {
        Task MockSeedInit();
        Task CreateMockCustomers();
        Task CreateMockDocuments();
    }
}