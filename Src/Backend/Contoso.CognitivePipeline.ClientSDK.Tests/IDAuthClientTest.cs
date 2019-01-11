using Contoso.CognitivePipeline.ClientSDK.Client;
using Contoso.CognitivePipeline.ClientSDK.Tests;
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
        
        [SetUp]
        public void Setup()
        {
            clientInstance = new IDAuthClient(Constants.APIManagementKey, Constants.APIManagementBaseUrl);
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
            string ownerId = Constants.OwnerId;
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

        [Test]
        public async Task SubmitInValidTest()
        {
            string ownerId = Constants.OwnerId;
            string expectedValue = "Petra Korica";
            string testFileName = "invalid_id.png";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.IDAuth(ownerId, doc, isAsync, isMinimum);
            Assert.IsInstanceOf<EmployeeId>(response, "response type is valid");
            Assert.IsFalse(response.IsAuthenticationSuccessful, "Authentication successful");
            Assert.AreEqual(response.EmployeeName, expectedValue, $"expected result ({expectedValue}) matched");
        }
    }
}