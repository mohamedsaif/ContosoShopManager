﻿// Source: https://github.com/Microsoft/app-innovation-workshop/blob/master/Backend/Monolithic/Helpers/SwaggerFilter.cs

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.API.Helpers
{
    public class SwaggerFilter : IDocumentFilter
    {
        private readonly string title;
        private readonly string filter;

        public SwaggerFilter(string title, string filter)
        {
            this.title = title;
            this.filter = filter;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            // Even though the filter is applied to a specific document they seem to get call for every document,
            // so to ensure we only apply the fitler to the right document we check it is for the expected document
            if (!swaggerDoc.Info.Title.Contains(this.title)) return;
            swaggerDoc.Paths = swaggerDoc.Paths.Where(x => x.Key.Contains(filter)).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
