using Contoso.SB.API.Abstractions;
using Contoso.CognitivePipeline.SharedModels.Models;
using Microsoft.Azure.Documents.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.SB.API.Mocks
{
    public class MockDataSeeder : IMockDataSeeder
    {
        IDocumentDBRepository<User> customerRepository;

        public MockDataSeeder(IDocumentDBRepository<User> custRepository)
        {
            customerRepository = custRepository;
        }

        public async Task MockSeedInit()
        {
            await CreateMockCustomers();
            await CreateMockDocuments();
        }

        public async Task CreateMockCustomers()
        {
            //Create new customer only if there is none
            //var count = customerRepository.GetItemsCount();
            var mockCustomer = await customerRepository.GetItemsAsync(c => c.ContactName == "Mohamed Saif");
            if (mockCustomer == null || mockCustomer.Count() == 0)
            {
                var cust = new User
                {
                    AccountId = "A9876-543210",
                    AccountType = "Employee",
                    Address = new Location
                    {
                        City = "Dubai",
                        Country = "UAE",
                        CreatedAt = DateTime.UtcNow,
                        Id = Guid.NewGuid().ToString(),
                        FirstLineAddress = "8th Street",
                        IsDeleted = false,
                        Point = new Point(25.2519659, 55.330614),
                        SecondLineAddress = "Dubai Deira City Center",
                        ZipCode = "1234"
                    },
                    //DigitalIdentity = new DigitalId {   },
                    AuthenticationOptions = "ADB2C",
                    DisplayName = "Mohamed Saif",
                    CompanyName = "Contoso",
                    ContactName = "Mohamed Saif",
                    ContactNumber = "+9711234567",
                    CreatedAt = DateTime.UtcNow,
                    Email = "mohamed.saif@outlook.com",
                    FacePersonId = "059d9c59-81c1-4bb1-96ba-244ee931a8a2", //TODO: Improve FacePersonId settings to be easily updated
                    Id = "4f93adac-13c6-4fe6-b625-afe7b7431fa1",
                    IsDeleted = false,
                    Website = "http://blog.mohamedsaif.com"
                };

                await customerRepository.CreateItemAsync(cust);
            }
        }

        public async Task CreateMockDocuments()
        {
            // TODO: Add some mock customers
        }
        
    }
}
