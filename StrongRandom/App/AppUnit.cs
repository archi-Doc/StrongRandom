﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using SimpleCommandLine;
using StrongRandom.PresentationState;
using static SimpleCommandLine.SimpleParser;

namespace StandardWinUI;

/// <summary>
/// AppUnit is a class that manages the dependencies of the DI container, logs, and CrystalData (data persistence).
/// </summary>
public class AppUnit : UnitBase, IUnitPreparable, IUnitExecutable
{
    public class Builder : UnitBuilder<Unit>
    {// Builder class for customizing dependencies.
        public Builder()
            : base()
        {
            // Configuration for Unit.
            this.Configure(context =>
            {
                // context.AddSingleton<AppUnit>();
                context.AddSingleton<StandardApp>();
                context.AddSingleton<IApp, App>();
                // context.Services.AddSingleton(x => (App)x.GetRequiredService<IApp>()); // If you want to use the App instance, please uncomment it.

                // CrossChannel
                context.Services.AddCrossChannel();

                // Presentation-State
                context.AddSingleton<NaviWindow>();
                context.AddSingleton<HomePage>();
                context.AddSingleton<HomePageState>();
                context.AddSingleton<SettingsPage>();
                context.AddSingleton<SettingsState>();
                context.AddSingleton<InformationPage>();
                context.AddSingleton<InformationState>();

                // Command
                // context.AddCommand(typeof(TestCommand));
                // context.AddCommand(typeof(TestCommand2));

                // Log filter
                context.AddSingleton<ExampleLogFilter>();

                // Logger
                context.ClearLoggerResolver();
                context.AddLoggerResolver(x =>
                {// Log source/level -> Resolver() -> Output/filter
                    x.SetOutput<FileLogger<FileLoggerOptions>>();

                    // if (x.LogLevel <= LogLevel.Debug)
                    // {
                    //    x.SetOutput<ConsoleLogger>();
                    //    return;
                    // }

                    // x.SetOutput<ConsoleAndFileLogger>();

                    // if (x.LogSourceType == typeof(TestCommand))
                    // {
                    //    x.SetFilter<ExampleLogFilter>();
                    // }
                });
            });

            this.PreConfigure(context =>
            {
                context.ProgramDirectory = Entrypoint.DataFolder;
                context.DataDirectory = Entrypoint.DataFolder;
            }).PostConfigure(context =>
            {
                var logfile = "Logs/Log.txt";
                context.SetOptions<FileLoggerOptions>(context.GetOptions<FileLoggerOptions>() with
                {// FileLoggerOptions
                    Path = Path.Combine(context.DataDirectory, logfile),
                    MaxLogCapacity = 2,
                    ClearLogsAtStartup = false,
                });
            });

            this.AddBuilder(CrystalBuilder());
        }

        private static CrystalControl.Builder CrystalBuilder()
        {
            return new CrystalControl.Builder()
                .ConfigureCrystal(context =>
                {
                    context.AddCrystal<AppSettings>(new()
                    {
                        NumberOfFileHistories = 0,
                        FileConfiguration = new GlobalFileConfiguration(AppSettings.Filename),
                        SaveFormat = SaveFormat.Utf8,
                    });
                });
        }
    }

    public class Unit : BuiltUnit
    {// Unit class for customizing behaviors.
        public record Param(string Args);

        public Unit(UnitContext context)
            : base(context)
        {
        }

        public async Task RunAsync(Param param)
        {
            // Create optional instances
            this.Context.CreateInstances();

            this.Context.SendPrepare(new());
            await this.Context.SendStartAsync(new(ThreadCore.Root));

            var parserOptions = SimpleParserOptions.Standard with
            {
                ServiceProvider = this.Context.ServiceProvider,
                RequireStrictCommandName = false,
                RequireStrictOptionName = true,
            };

            // Main
            await SimpleParser.ParseAndRunAsync(this.Context.Commands, param.Args, parserOptions);

            this.Context.SendStop(new());
            await this.Context.SendTerminateAsync(new());
        }
    }

    private class ExampleLogFilter : ILogFilter
    {
        public ExampleLogFilter(AppUnit consoleUnit)
        {
            this.consoleUnit = consoleUnit;
        }

        public ILogWriter? Filter(LogFilterParameter param)
        {// Log source/Event id/LogLevel -> Filter() -> ILog
            if (param.LogSourceType == typeof(StandardApp))
            {
                // return null; // No log
                if (param.LogLevel == LogLevel.Error)
                {
                    return param.Context.TryGet<ConsoleAndFileLogger>(LogLevel.Fatal); // Error -> Fatal
                }
                else if (param.LogLevel == LogLevel.Fatal)
                {
                    return param.Context.TryGet<ConsoleAndFileLogger>(LogLevel.Error); // Fatal -> Error
                }
            }

            return param.OriginalLogger;
        }

        private AppUnit consoleUnit;
    }

    public AppUnit(UnitContext context, ILogger<AppUnit> logger, UnitOptions options)
        : base(context)
    {
        this.logger = logger;
        this.options = options;
    }

    void IUnitPreparable.Prepare(UnitMessage.Prepare message)
    {
        this.logger.TryGet()?.Log("Unit prepared.");
        this.logger.TryGet()?.Log($"Program: {this.options.ProgramDirectory}");
        this.logger.TryGet()?.Log($"Data: {this.options.DataDirectory}");
    }

    async Task IUnitExecutable.StartAsync(UnitMessage.StartAsync message, CancellationToken cancellationToken)
    {
        this.logger.TryGet()?.Log("Unit started.");
    }

    void IUnitExecutable.Stop(UnitMessage.Stop message)
    {
        this.logger.TryGet()?.Log("Unit stopped.");
    }

    async Task IUnitExecutable.TerminateAsync(UnitMessage.TerminateAsync message, CancellationToken cancellationToken)
    {
        this.logger.TryGet()?.Log("Unit terminated.");
    }

    private readonly ILogger logger;
    private readonly UnitOptions options;
}
