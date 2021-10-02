# Serilog.Sinks.LiteDb.Async
A lightweight high performance Serilog sink that writes to LiteDb database.

## Getting started
Install [Serilog.Sinks.LiteDb.Async](https://www.nuget.org/packages/Serilog.Sinks.LiteDb.Async) from NuGet

```PowerShell
Install-Package Serilog.Sinks.LiteDb.Async
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


You will find more examples in the repo:

[PavlovRconWebserver](https://github.com/devinSpitz/PavlovRconWebserver)

ps. currently only on the TestBranch.


Donate:
=======
Feel free to support my work by donating:

<a href="https://www.paypal.com/donate?hosted_button_id=JYNFKYARZ7DT4">
<img src="https://www.paypalobjects.com/en_US/CH/i/btn/btn_donateCC_LG.gif" alt="Donate with PayPal" />
</a>

Business:
=======

For business inquiries please use:

<a href="mailto:&#x64;&#x65;&#x76;&#x69;&#x6e;&#x40;&#x73;&#x70;&#x69;&#x74;&#x7a;&#x65;&#x6e;&#x2e;&#x73;&#x6f;&#x6c;&#x75;&#x74;&#x69;&#x6f;&#x6e;&#x73;">&#x64;&#x65;&#x76;&#x69;&#x6e;&#x40;&#x73;&#x70;&#x69;&#x74;&#x7a;&#x65;&#x6e;&#x2e;&#x73;&#x6f;&#x6c;&#x75;&#x74;&#x69;&#x6f;&#x6e;&#x73;</a>


References:
=======
- LiteDB - [https://github.com/quicksln/LiteDB#](https://github.com/quicksln/LiteDB#)
- Async LiteDb - [https://github.com/mlockett42/litedb-async#](https://github.com/mlockett42/litedb-async#)
- Newtonsoft.Json - [https://github.com/JamesNK/Newtonsoft.Json#](https://github.com/JamesNK/Newtonsoft.Json#)
- Serilog - [https://github.com/serilog/serilog#](https://github.com/serilog/serilog#)


Where to use it ?
=======
- Great for small and medium size AspNetCore Websites,

License
=======

[MIT](http://opensource.org/licenses/MIT)


