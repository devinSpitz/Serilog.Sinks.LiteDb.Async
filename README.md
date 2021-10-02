# Serilog.Sinks.LiteDb.Async
A lightweight high performance Serilog sink that writes to LiteDb database.

## Getting started
Install [Serilog.Sinks.LiteDb.Async](https://www.nuget.org/packages/Serilog.Sinks.LiteDb.Async) from NuGet

```PowerShell
Install-Package Serilog.Sinks.LiteDb
```

## Configure logger

in the Program.cs:
```C#
        public static void Main(string[] args)
        {
            var configSettings = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .ReadFrom.Configuration(configSettings)
                .WriteTo.LiteDbAsync(configSettings.GetConnectionString("DefaultConnection")) // This line does active the db logging
                .CreateBootstrapLogger();
            
            Log.Information("Starting up!");

            try
            {
                CreateHostBuilder(args).Build().Run();

                Log.Information("Stopped cleanly");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


```
And 

```C#
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // This line activates Serilog
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
```


You can also hardcode the connectionString:

```C#
    .WriteTo.LiteDbAsync("Filename=Database.db;Connection=shared;") 
```