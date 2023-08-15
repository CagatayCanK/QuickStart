using System.Diagnostics;
using Elasticsearch.Net;
using Serilog.Debugging;
using Serilog.Sinks.Elasticsearch;

namespace QuickStart.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

        Log.Information("Starting up!");

        try
        {
            CreateHostBuilder(args).Build().Run();

            Log.Information("Stopped cleanly");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext())
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}
