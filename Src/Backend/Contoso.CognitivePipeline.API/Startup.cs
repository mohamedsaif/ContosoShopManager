using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contoso.SB.API.Abstractions;
using Contoso.SB.API.Data;
using Contoso.SB.API.Mocks;
using Contoso.CognitivePipeline.SharedModels.Models;
using Contoso.SB.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Contoso.SB.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc();

            //Register application services
            services.AddSingleton<IStorageRepository, AzureBlobStorageRepository>();
            services.AddSingleton<IQueueRepository, AzureQueueStorageRepository>();
            services.AddTransient<INewCognitiveRequest<SmartDoc>, NewCognitiveReqService<SmartDoc>>();
            services.AddTransient<IDocumentDBRepository<SmartDoc>, CosmosDBRepository<SmartDoc>>();
            services.AddTransient<IDocumentDBRepository<User>, CosmosDBRepository<User>>();

            //Generate mock data service
            services.AddTransient<IMockDataSeeder, MockDataSeeder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IMockDataSeeder mockSeeder)
        {
            //Seeding mock data
            mockSeeder.MockSeedInit().Wait();

            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseMvc();
        }
    }
}
