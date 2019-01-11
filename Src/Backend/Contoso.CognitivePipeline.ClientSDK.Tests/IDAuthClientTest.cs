using Contoso.CognitivePipeline.ClientSDK.Client;
using Contoso.CognitivePipeline.ClientSDK.Tests;
using Contoso.CognitivePipeline.ClientSDK.Tests.Helpers;
using Contoso.CognitivePipeline.SharedModels.Models;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class IDAuthClientTest : BaseClientTest<IDAuthClient, EmployeeId>
    {
        [Test]
        public async Task SubmitValidCorrectId()
        {
            string ownerId = Constants.OwnerId;
            string expectedValue = "Mohamed Saif";
            string testFileName = "valid_id.png";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.IDAuth(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsTrue(response.IsAuthenticationSuccessful, "Authentication successful");
            Assert.AreEqual(response.EmployeeName, expectedValue, $"expected result ({expectedValue}) matched");
        }

        [Test]
        public async Task SubmiteValidIncorrectId()
        {
            string ownerId = Constants.OwnerId;
            string expectedValue = "Petra Korica";
            string testFileName = "invalid_id.png";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.IDAuth(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsFalse(response.IsAuthenticationSuccessful, "Authentication unsuccessful");
            Assert.AreEqual(response.EmployeeName, expectedValue, $"expected result ({expectedValue}) matched");
        }

        [Test]
        public async Task SubmitInvalidId()
        {
            string ownerId = Constants.OwnerId;
            string expectedValue = null;
            string testFileName = "invalid1.jpg";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.IDAuth(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsFalse(response.IsAuthenticationSuccessful, "Authentication successful");
            Assert.AreEqual(response.EmployeeName, expectedValue, $"expected result ({expectedValue}) matched");
        }
    }
}