using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Contoso.SB.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();

            //Adding additional configuration for the WebHost
            //UseContentRoot(Directory.GetCurrentDirectory())
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //All these configuration and more are part of the default builder
                //.UseKestrel()
                //.UseContentRoot(Directory.GetCurrentDirectory())
                //.UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
    }
}
