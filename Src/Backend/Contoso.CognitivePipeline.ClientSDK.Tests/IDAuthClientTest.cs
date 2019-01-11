using Contoso.CognitivePipeline.ClientSDK.Client;
using Contoso.CognitivePipeline.ClientSDK.Tests.Helpers;
using Contoso.CognitivePipeline.SharedModels.Models;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class IDAuthClientTest
    {
        private IDAuthClient clientInstance;
        private string baseUrl = "https://contoso-shop-api.azure-api.net";
        private string key = "afd8a5e59fd64e1eafddfbbcf7b77f55";

        [SetUp]
        public void Setup()
        {
            clientInstance = new IDAuthClient(key, baseUrl);
        }

        [TearDown]
        public void Cleanup()
        {
            clientInstance = null;
        }

        [Test]
        public void InstanceTest()
        {
            var clientType = typeof(IDAuthClient);
            Assert.IsInstanceOf(clientType, clientInstance, $"instance is a {clientType.Name}");
        }

        [Test]
        public async Task SubmitValidTest()
        {
            string ownerId = "4f93adac-13c6-4fe6-b625-afe7b7431fa1";
            string expectedValue = "Mohamed Saif";
            string testFileName = "valid_id.png";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.IDAuth(ownerId, doc, isAsync, isMinimum);
            Assert.IsInstanceOf<EmployeeId> (response, "response type is valid");
            Assert.IsTrue(response.IsAuthenticationSuccessful, "Authentication successful");
            Assert.AreEqual(response.EmployeeName, expectedValue, $"expected result ({expectedValue}) matched");
        }
    }
}