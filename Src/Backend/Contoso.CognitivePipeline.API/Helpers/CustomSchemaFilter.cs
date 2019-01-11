using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.API.Helpers
{
    public class CustomSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            //This is to allow easy import to Azure API Management service by resting interface like IFormFile.Headers object type to string.
            var typeInfo = context.SystemType.GetTypeInfo();
            if (typeInfo.IsInterface)
                schema.Type = "string";
        }
    }
}
