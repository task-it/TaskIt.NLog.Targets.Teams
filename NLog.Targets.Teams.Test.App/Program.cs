﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;

namespace NLog.Targets.Teams.Test.App
{
    class Program
    {
        /// <summary>
        /// DO nothing
        /// </summary>
        private Program()
        {
        }

        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var config = new ConfigurationBuilder()
                   .SetBasePath(System.IO.Directory.GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
                   .AddJsonFile("appSettingDemo.json", optional: true, reloadOnChange: true)
                   .Build();

                using var servicesProvider = new ServiceCollection()
                   .AddTransient<Runner>() // Runner is the custom class
                   .AddLogging(loggingBuilder =>
                   {
                       // configure Logging with NLog
                       loggingBuilder.ClearProviders();
                       loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                       loggingBuilder.AddNLog(config);
                   })
                   .BuildServiceProvider();

                // comment this in to test the different log levels
                var runner = servicesProvider.GetRequiredService<Runner>();
                try
                {
                    runner.DoActions();
                }
                catch (InvalidOperationException e)
                {
                    logger.Fatal(e, "oO ... something is wrong, we're all doomed");
                }

                Console.WriteLine("Press ANY key to exit");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }
    }
}
