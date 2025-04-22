using Serilog;
using WebApi;
using Microsoft.Extensions.Configuration;

public class Program
{
    static void Main(string[] args)
    {
        // Set up configuration to read appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // This points to the root directory of your project
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Read Serilog configuration from appsettings.json
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration) // This loads the Serilog configuration from the appsettings.json
            .CreateLogger();

        try
        {
            Log.Information("Starting up the web application");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();
            builder.Services.AddHostedService<DrawingProcessor>();

            var app = builder.Build();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

