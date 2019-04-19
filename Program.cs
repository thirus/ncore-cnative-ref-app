using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;
using Prometheus.DotNetRuntime;

namespace ReferenceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // DotNetRuntimeStatsBuilder.Default().WithErrorHandler(e =>
            //         {
            //             Console.WriteLine(e.ToString());
            //         }).StartCollecting();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
