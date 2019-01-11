using Contoso.CognitivePipeline.ClientSDK.Base;
using Contoso.CognitivePipeline.ClientSDK.Client;
using Contoso.CognitivePipeline.ClientSDK.Tests;
using Contoso.CognitivePipeline.ClientSDK.Tests.Helpers;
using Contoso.CognitivePipeline.SharedModels.Models;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public abstract class BaseClientTest<T, R> 
        where T : BaseCognitiveApi
        where R : SmartDoc
    {
        protected T clientInstance;

        [SetUp]
        public void Setup()
        {
            clientInstance = (T)Activator.CreateInstance(typeof(T), Constants.APIManagementKey, Constants.APIManagementBaseUrl);
        }

        [TearDown]
        public void Cleanup()
        {
            clientInstance = null;
        }

        [Test]
        public void InstanceTest()
        {
            var clientType = typeof(T);
            Assert.IsInstanceOf(clientType, clientInstance, $"instance is a {clientType.Name}");
        }

        public void IsResultTypeValid(R result)
        {
            Assert.IsInstanceOf<R>(result, "response type is valid");
        }
    }
}