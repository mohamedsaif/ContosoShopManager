using Contoso.CognitivePipeline.ClientSDK.Client;
using Contoso.CognitivePipeline.ClientSDK.Tests;
using Contoso.CognitivePipeline.ClientSDK.Tests.Helpers;
using Contoso.CognitivePipeline.SharedModels.Models;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class FaceAuthClientTest : BaseClientTest<FaceAuthClient, FaceAuthCard>
    {
        [Test]
        public async Task SubmitValidCorrectFace()
        {
            string ownerId = Constants.OwnerId;
            string expectedValue = "Mohamed Saif";
            string testFileName = "valid_id.png";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.FaceAuth(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsTrue(response.IsAuthenticationSuccessful, "Authentication successful");
            Assert.AreEqual(response.DetectedFaceName, expectedValue, $"expected result ({expectedValue}) matched");
        }

        [Test]
        public async Task SubmiteValidIncorrectFace()
        {
            string ownerId = Constants.OwnerId;
            string expectedValue = null;
            string testFileName = "invalid_id.png";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.FaceAuth(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsFalse(response.IsAuthenticationSuccessful, "Authentication unsuccessful");
            Assert.IsTrue(string.IsNullOrEmpty(response.DetectedFaceName), $"expected result ({expectedValue}) matched");
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
            var response = await clientInstance.FaceAuth(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsFalse(response.IsAuthenticationSuccessful, "Authentication unsuccessful");
            Assert.IsTrue(string.IsNullOrEmpty(response.DetectedFaceName), $"expected result ({expectedValue}) matched");
        }
    }
}