using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Health;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;

namespace ReferenceApp
{
    public class Program
    {
        public static IMetricsRoot Metrics { get; set; }

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Elasticsearch().WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                        {
                            MinimumLogEventLevel = LogEventLevel.Verbose,
                            AutoRegisterTemplate = true,
                        })
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            finally
            {
                // Close and flush the log.
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            Metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusPlainText()
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(Configuration)
                .UseSerilog()
                .UseHealth()
                .ConfigureMetrics(Metrics)
                .UseMetrics(options =>
                        {
                            options.EndpointOptions = endpointsOptions =>
                            {
                                endpointsOptions.MetricsEndpointOutputFormatter = Metrics.OutputMetricsFormatters.GetType<MetricsPrometheusTextOutputFormatter>();
                            };
                        })
                .UseMetricsWebTracking()
                .UseStartup<Startup>();
        }
    }
}
