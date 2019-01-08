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
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Contoso.CognitivePipeline.API.Helpers;

namespace Contoso.SB.API
{
    public class Startup
    {
        List<string> apis = new List<string>
        {
            "IDAuth",
            "FaceAuth",
            "ShelvesCompliance",
            "Classification",
        };
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc();

            //Add API documentation service
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Info { Title = "Contoso Cognitive Pipeline API", Version = "v1" });
            //    var filePath = Path.Combine(System.AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly().GetName().Name}.xml");
            //    c.IncludeXmlComments(filePath);
            //});
            foreach(var api in apis)
            {
                services.AddSwaggerGen(this.SwaggerGen(api, $"api/{api.ToLower()}"));
            }
            //services.AddSwaggerGen(this.SwaggerGen("EmpIdAuth", "api/idauth"));
            //services.AddSwaggerGen(this.SwaggerGen("FaceAuth", "api/faceauth"));
            //services.AddSwaggerGen(this.SwaggerGen("Shelves", "api/shelves"));
            //services.AddSwaggerGen(this.SwaggerGen("Classification", "api/classification"));

            //Register application services
            services.AddSingleton<IStorageRepository, AzureBlobStorageRepository>();
            services.AddSingleton<IQueueRepository, AzureQueueStorageRepository>();
            services.AddTransient<INewCognitiveRequest<SmartDoc>, NewCognitiveReqService<SmartDoc>>();
            services.AddTransient<IDocumentDBRepository<SmartDoc>, CosmosDBRepository<SmartDoc>>();
            services.AddTransient<IDocumentDBRepository<User>, CosmosDBRepository<User>>();

            //Generate mock data service
            services.AddTransient<IMockDataSeeder, MockDataSeeder>();
        }

        private Action<SwaggerGenOptions> SwaggerGen(string name, string filter)
        {
            return c =>
            {
                c.SwaggerDoc(name, new Info { Title = $"Contoso Smart Shop API - {name}", Version = "1.0" });

                // The swagger filter takes the filter and removes any path that doesn't contain 
                // the value of the filter in its route. Allowing for us to seperate out the generated
                // swagger documents
                c.DocumentFilter<SwaggerFilter>(name, filter);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IMockDataSeeder mockSeeder)
        {
            //Seeding mock data
            mockSeeder.MockSeedInit().Wait();

            loggerFactory.AddConsole();

            //As this is a demo, we will continue to use Developer Exceptions. 
            app.UseDeveloperExceptionPage();

            //In production you would change that.
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler();
            //}

            app.UseMvc();

            //Configuring Swagger API documentation and its UI
            app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contoso Cognitive Pipeline V1");
            //});
            app.UseSwaggerUI(c =>
            {
                foreach(var api in apis)
                {
                    c.SwaggerEndpoint($"/swagger/{api}/swagger.json", api);
                }
                //c.SwaggerEndpoint("/swagger/EmpIdAuth/swagger.json", "EmpIdAuth");
                //c.SwaggerEndpoint("/swagger/FaceAuth/swagger.json", "Parts");
                //c.SwaggerEndpoint("/swagger/Shelves/swagger.json", "Dummy");
                //c.SwaggerEndpoint("/swagger/Classification/swagger.json", "Photo");
                //c.SwaggerEndpoint("/swagger/Search/swagger.json", "Search");
                c.RoutePrefix = string.Empty; // Makes Swagger UI the root page
            });
        }
    }
}
