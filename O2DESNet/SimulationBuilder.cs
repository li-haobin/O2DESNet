using System;
using System.Collections.Generic;
using System.IO;

using Serilog;
using Serilog.Events;

namespace O2DESNet;

/// <summary>
/// Fluent builder for configuring and returning an ISandbox instance.
/// Usage:
///   var sim = new SimulationBuilder<MyStartup>(rootSandbox)
///                 .AddSimulationStatics(myStatics)
///                 .AddLogger()
///                 .Build();
/// </summary>
/// <typeparam name="TStartup">A marker startup type for the simulation configuration pipeline.</typeparam>
public sealed class SimulationBuilder<TStartup> where TStartup : class
{
    private readonly ISandbox _root;
    private readonly Dictionary<Type, object> _statics = new Dictionary<Type, object>();

    // Logging configuration
    private bool _enableConsole = true;
    private string _filePath;
    private LogEventLevel _minLevel = LogEventLevel.Information;
    private bool _loggerConfigured;

    public SimulationBuilder(ISandbox root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    /// <summary>
    /// Adds a statics/configuration object that implements IAssets.
    /// </summary>
    public SimulationBuilder<TStartup> AddSimulationStatics<TStatics>(TStatics statics)
        where TStatics : class, IAssets
    {
        if (statics == null)
            throw new ArgumentNullException(nameof(statics));
        _statics[typeof(TStatics)] = statics;
        return this;
    }

    /// <summary>
    /// Configure Serilog logger. By default writes to Console.
    /// Set filePath to also write logs into a rolling file.
    /// </summary>
    public SimulationBuilder<TStartup> AddLogger(string filePath = null, bool writeToConsole = true, LogEventLevel minLevel = LogEventLevel.Information)
    {
        _filePath = filePath;
        _enableConsole = writeToConsole;
        _minLevel = minLevel;
        _loggerConfigured = true;
        return this;
    }

    /// <summary>
    /// Try to get a statics object previously added.
    /// </summary>
    public bool TryGetStatics<TStatics>(out TStatics result) where TStatics : class, IAssets
    {
        object value;
        if (_statics.TryGetValue(typeof(TStatics), out value))
        {
            result = (TStatics)value;
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>
    /// Finalize configuration and return the configured root ISandbox.
    /// </summary>
    public ISandbox Build()
    {
        if (_loggerConfigured)
        {
            ConfigureSerilog();
        }

        return _root;
    }

    private void ConfigureSerilog()
    {
        var cfg = new LoggerConfiguration()
            .MinimumLevel.Is(_minLevel)
            .Enrich.FromLogContext();

        if (_enableConsole)
        {
            cfg = cfg.WriteTo.Console();
        }

        if (!string.IsNullOrWhiteSpace(_filePath))
        {
            // Ensure directory exists to avoid exceptions
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            cfg = cfg.WriteTo.File(
                _filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                restrictedToMinimumLevel: _minLevel,
                shared: true);
        }

        Log.Logger = cfg.CreateLogger();
    }
}
