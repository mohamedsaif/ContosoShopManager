using Contoso.CognitivePipeline.ClientSDK.Client;
using Contoso.CognitivePipeline.ClientSDK.Tests;
using Contoso.CognitivePipeline.ClientSDK.Tests.Helpers;
using Contoso.CognitivePipeline.SharedModels.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class ShelvesComplianceClientTest : BaseClientTest<ShelvesComplianceClient, ShelfCompliance>
    {
        [Test]
        public async Task SubmitValidCompliantShelves()
        {
            string ownerId = Constants.OwnerId;
            var testFileName = "valid1.jpg";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.ValidateShelvesCompliace(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsTrue(response.IsConfidenceAcceptable, "Classification threshold passed");
            Assert.IsTrue(response.IsCompliant, $"Classification of {response.DetectionNotes}");
        }

        [Test]
        public async Task SubmitInvalidCompliantShelves()
        {
            string ownerId = Constants.OwnerId;
            var testFileName = "invalid1.jpg";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.ValidateShelvesCompliace(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsTrue(response.IsConfidenceAcceptable, "Classification threshold passed");
            Assert.IsFalse(response.IsCompliant, $"Classification of {response.DetectionNotes}");
        }

        [Test]
        public async Task SubmitInvalidImage()
        {
            string ownerId = Constants.OwnerId;
            var testFileName = "valid_id.png";
            byte[] doc = TestFilesHelper.GetTestFile(testFileName);
            bool isAsync = false;
            bool isMinimum = true;
            var response = await clientInstance.ValidateShelvesCompliace(ownerId, doc, isAsync, isMinimum);
            IsResultTypeValid(response);
            Assert.IsFalse(response.IsConfidenceAcceptable, $"Classification threshold failed.  {response.DetectionNotes}");
        }
    }
}
