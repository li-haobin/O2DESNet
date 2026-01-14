using Microsoft.Extensions.Logging;

using NUnit.Framework;

using O2DESNet.Standard;

using Serilog;
using Serilog.Events;

using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.UnitTests.PmPathTests;

[TestFixture]
public partial class PmPathTraffic_Tests
{
    private Microsoft.Extensions.Logging.ILogger? _logger;
    private LogEventLevel _minLevel = LogEventLevel.Information;

    [SetUp]
    public void Init()
    {
        // Comment out the following code to switch log file
        ConfigureSerilog("Logs\\log-O2DES-Library-Tests.txt");
    }

    private void ConfigureSerilog(string filePath)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(_minLevel)
            .WriteTo.Console()      // Output to console
            .WriteTo.File(filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                restrictedToMinimumLevel: _minLevel,
                shared: true)       // Output to file
            .Enrich.FromLogContext()
            .CreateLogger();

        _logger = new LoggerFactory()
            .AddSerilog(Log.Logger)
            .CreateLogger<Simulator_Tests>();
    }

    [Test]
    public void PmPath_Module_Single_Capacity_Test()
    {
        // Arrange
        var path1 = new PmPathSingleCapacity("Path1", 0);
        path1.OnArrive += (s, e) =>
        {
            var vehicle = e.Vehicle as Vehicle;
            var path = s as PmPathSingleCapacity;
            _logger?.LogInformation($"{vehicle?.Name} arrived at {path?.Id} at {path?.ClockTime}");
        };

        path1.OnProcess += (s, e) =>
        {
            var vehicle = e.Vehicle as Vehicle;
            var path = s as PmPathSingleCapacity;
            _logger?.LogInformation($"{vehicle?.Name} processing at {path?.Id} at {path?.ClockTime}");
        };

        path1.OnDepart += (s, e) =>
        {
            var vehicle = e.Vehicle as Vehicle;
            var path = s as PmPathSingleCapacity;
            _logger?.LogInformation($"{vehicle?.Name} departed from {path?.Id} at {path?.ClockTime}");
        };

        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle1"), TimeSpan.FromSeconds(1));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle2"), TimeSpan.FromSeconds(5));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle3"), TimeSpan.FromSeconds(25));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle4"), TimeSpan.FromSeconds(27));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle5"), TimeSpan.FromSeconds(30));

        path1.Run(15);

        path1.Dispose();

    }

    [Test]
    public void PmPath_Module_N_Capacity_Test()
    {
        double vehicleLength = 5; // meters
        double vehicleAllowedGapDistance = 2; // meters
        int capacity = 2;

        // Arrange
        var path1 = new PmPathMultiCapacity("Path1", 0, vehicleLength, vehicleAllowedGapDistance, capacity);

        _logger?.LogInformation("Begin two-capacity path validation");
        _logger?.LogInformation($"Configured Capacity={path1.Capacity}, ProcessDuration={path1.ProcessDuration}, MinHeadway={path1.MinHeadwayTime}");

        // Collect events for verification
        var arrivals = new List<(Vehicle vehicle, TimeSpan time)>();
        var starts = new List<(Vehicle vehicle, TimeSpan time)>();
        var departures = new List<(Vehicle vehicle, TimeSpan time)>();

        path1.OnArrive += (s, e) =>
        {
            var vehicle = e.Vehicle as Vehicle;
            var path = (PmPathMultiCapacity)s!;
            arrivals.Add((vehicle!, path.ClockTime));
            _logger?.LogInformation($"Event: {vehicle?.Name} arrived at {path.ClockTime}");
        };

        path1.OnProcess += (s, e) =>
        {
            var vehicle = e.Vehicle as Vehicle;
            var path = (PmPathMultiCapacity)s!;
            starts.Add((vehicle!, path.ClockTime));
            _logger?.LogInformation($"Event: {vehicle?.Name} started at {path.ClockTime}");
        };

        path1.OnDepart += (s, e) =>
        {
            var vehicle = e.Vehicle as Vehicle;
            var path = (PmPathMultiCapacity)s!;
            departures.Add((vehicle!, path.ClockTime));
            _logger?.LogInformation($"Event: {vehicle?.Name} departed at {path.ClockTime}");
        };

        // Enqueue several vehicles densely to exercise N-capacity with spacing
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle1"), TimeSpan.FromSeconds(1));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle2"), TimeSpan.FromSeconds(5));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle3"), TimeSpan.FromSeconds(25));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle4"), TimeSpan.FromSeconds(27));
        path1.RequestToEnter(new Vehicle(VehicleId.New(), "Vehicle5"), TimeSpan.FromSeconds(30));

        // Run enough events to process all
        path1.Run(40);

        _logger?.LogInformation("Validation step: counts should match the number of vehicles");
        Assert.That(arrivals.Count, Is.EqualTo(5), "All vehicles should arrive");
        Assert.That(starts.Count, Is.EqualTo(5), "All vehicles should start processing");
        Assert.That(departures.Count, Is.EqualTo(5), "All vehicles should depart");

        // Build lookup for times
        var startMap = starts.ToDictionary(x => x.vehicle, x => x.time);
        var departMap = departures.ToDictionary(x => x.vehicle, x => x.time);

        _logger?.LogInformation("Validation step: each depart time equals start + ProcessDuration");
        foreach (var (vehicle, startTime) in starts)
        {
            Assert.That(departMap.ContainsKey(vehicle), $"Missing depart for {vehicle.Name}");
            var departTime = departMap[vehicle];
            _logger?.LogInformation($"Check duration for {vehicle.Name}: start={startTime}, depart={departTime}");
            Assert.That(departTime - startTime, Is.EqualTo(path1.ProcessDuration), $"Process duration mismatch for {vehicle.Name}");
        }

        _logger?.LogInformation("Validation step: headway between consecutive starts >= MinHeadway");
        var orderedStarts = starts.OrderBy(x => x.time).ToList();
        for (int i = 1; i < orderedStarts.Count; i++)
        {
            var prev = orderedStarts[i - 1];
            var curr = orderedStarts[i];
            var delta = curr.time - prev.time;
            _logger?.LogInformation($"Headway {prev.vehicle.Name}->{curr.vehicle.Name}: {delta} (min {path1.MinHeadwayTime})");
            Assert.That(delta >= path1.MinHeadwayTime, $"Headway violated between {prev.vehicle.Name} and {curr.vehicle.Name}: {delta} < {path1.MinHeadwayTime}");
        }

        _logger?.LogInformation("Validation step: concurrency never exceeds Capacity");
        var changes = new List<(TimeSpan t, int d)>();
        changes.AddRange(starts.Select(s => (s.time, +1)));
        changes.AddRange(departures.Select(d => (d.time, -1)));
        var ordered = changes
            .OrderBy(c => c.t)
            .ThenBy(c => c.d) // departures (-1) before starts (+1) at the same timestamp
            .ToList();

        int concurrent = 0;
        int maxConcurrent = 0;
        foreach (var (t, d) in ordered)
        {
            concurrent += d;
            if (concurrent > maxConcurrent)
                maxConcurrent = concurrent;

            Assert.That(concurrent, Is.LessThanOrEqualTo(path1.Capacity), $"Capacity exceeded (>{path1.Capacity}) at {t}");
        }

        _logger?.LogInformation($"Max concurrency observed: {maxConcurrent}");
        Assert.That(maxConcurrent, Is.LessThanOrEqualTo(path1.Capacity), $"Max concurrency should be <= {path1.Capacity}");

        _logger?.LogInformation("N-capacity path validation completed successfully");

        path1.Dispose();
    }

    class PmPathTrafficNetwork : Sandbox
    {
        public event EventHandler<VehicleArrivedEventArgs>? OnArrive;
        public event EventHandler<VehicleProcessedEventArgs>? OnProcess;
        public event EventHandler<VehicleDepartedEventArgs>? OnDepart;

        // Private Fields
        private readonly double _vehicleLengthMeters;
        private readonly double _vehicleAllowedGapMeters;
        private readonly double _speedMetersPerSecond = 1d; // assumed constant speed along path
        private readonly TimeSpan _processDuration = TimeSpan.FromSeconds(10);
        private readonly int _capacity;
        private readonly List<TimeSpan> _serverNexts;

        // Reserve headway based on the last planned start time
        private TimeSpan? _lastPlannedStartTime;

        // Public Properties
        public TimeSpan ProcessDuration => _processDuration;
        public TimeSpan MinHeadwayTime => MinHeadway();
        public int Capacity => _capacity;

        public PmPathTrafficNetwork(string id, int seed, double vehicleLength, double vehicleAllowedGapDistance, int capacity) : base(id, seed)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive");

            _vehicleLengthMeters = vehicleLength;
            _vehicleAllowedGapMeters = vehicleAllowedGapDistance;
            _capacity = capacity;
            _serverNexts = Enumerable.Repeat(TimeSpan.Zero, _capacity).ToList();
        }

        public void RequestToEnter(Vehicle vehicle, TimeSpan timestamp)
        {
            Schedule(() => Arrive(vehicle), timestamp);
        }

        private TimeSpan MinHeadway()
        {
            var seconds = (_vehicleLengthMeters + _vehicleAllowedGapMeters) / _speedMetersPerSecond;
            return TimeSpan.FromSeconds(seconds);
        }

        private static TimeSpan Max(TimeSpan a, TimeSpan b)
        {
            return a >= b ? a : b;
        }

        private static TimeSpan Max(TimeSpan a, TimeSpan b, TimeSpan c)
        {
            return Max(Max(a, b), c);
        }

        private void Arrive(IEntity vehicle)
        {
            OnArrive?.Invoke(this, new VehicleArrivedEventArgs(vehicle, null));

            var headwayEarliest = _lastPlannedStartTime is null ? TimeSpan.Zero : _lastPlannedStartTime.Value + MinHeadway();
            // Find the earliest available server index and time
            int earliestIdx = 0;
            var earliestServerTime = _serverNexts[0];
            for (int i = 1; i < _serverNexts.Count; i++)
            {
                if (_serverNexts[i] < earliestServerTime)
                {
                    earliestServerTime = _serverNexts[i];
                    earliestIdx = i;
                }
            }

            var startTime = Max(ClockTime, headwayEarliest, earliestServerTime);
            var delay = startTime - ClockTime;

            // Reserve capacity now by assigning the earlier-available server
            _serverNexts[earliestIdx] = startTime + _processDuration;

            // Reserve headway for subsequent arrivals
            _lastPlannedStartTime = startTime;

            Schedule(() => Process(vehicle), delay);
        }

        private void Process(IEntity vehicle)
        {
            OnProcess?.Invoke(this, new VehicleProcessedEventArgs(vehicle, null));
            Schedule(() => Depart(vehicle), _processDuration);
        }

        private void Depart(IEntity vehicle)
        {
            OnDepart?.Invoke(this, new VehicleDepartedEventArgs(vehicle, null));
        }
    }

    class PmPathMultiCapacity : Sandbox
    {
        public event EventHandler<VehicleArrivedEventArgs>? OnArrive;
        public event EventHandler<VehicleProcessedEventArgs>? OnProcess;
        public event EventHandler<VehicleDepartedEventArgs>? OnDepart;

        // Private Fields
        private readonly double _vehicleLengthMeters;
        private readonly double _vehicleAllowedGapMeters;
        private readonly double _speedMetersPerSecond = 1d; // assumed constant speed along path
        private readonly TimeSpan _processDuration = TimeSpan.FromSeconds(10);
        private readonly int _capacity;
        private readonly List<TimeSpan> _serverNexts;

        // Reserve headway based on the last planned start time
        private TimeSpan? _lastPlannedStartTime;

        // Public Properties
        public TimeSpan ProcessDuration => _processDuration;
        public TimeSpan MinHeadwayTime => MinHeadway();
        public int Capacity => _capacity;

        public PmPathMultiCapacity(string id, int seed, double vehicleLength, double vehicleAllowedGapDistance, int capacity) : base(id, seed)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive");

            _vehicleLengthMeters = vehicleLength;
            _vehicleAllowedGapMeters = vehicleAllowedGapDistance;
            _capacity = capacity;
            _serverNexts = Enumerable.Repeat(TimeSpan.Zero, _capacity).ToList();
        }

        public void RequestToEnter(Vehicle vehicle, TimeSpan timestamp)
        {
            Schedule(() => Arrive(vehicle), timestamp);
        }

        private TimeSpan MinHeadway()
        {
            var seconds = (_vehicleLengthMeters + _vehicleAllowedGapMeters) / _speedMetersPerSecond;
            return TimeSpan.FromSeconds(seconds);
        }

        private static TimeSpan Max(TimeSpan a, TimeSpan b)
        {
            return a >= b ? a : b;
        }

        private static TimeSpan Max(TimeSpan a, TimeSpan b, TimeSpan c)
        {
            return Max(Max(a, b), c);
        }

        private void Arrive(Vehicle vehicle)
        {
            OnArrive?.Invoke(this, new VehicleArrivedEventArgs(vehicle, null));

            var headwayEarliest = _lastPlannedStartTime is null ? TimeSpan.Zero : _lastPlannedStartTime.Value + MinHeadway();
            // Find the earliest available server index and time
            int earliestIdx = 0;
            var earliestServerTime = _serverNexts[0];
            for (int i = 1; i < _serverNexts.Count; i++)
            {
                if (_serverNexts[i] < earliestServerTime)
                {
                    earliestServerTime = _serverNexts[i];
                    earliestIdx = i;
                }
            }

            var startTime = Max(ClockTime, headwayEarliest, earliestServerTime);
            var delay = startTime - ClockTime;

            // Reserve capacity now by assigning the earlier-available server
            _serverNexts[earliestIdx] = startTime + _processDuration;

            // Reserve headway for subsequent arrivals
            _lastPlannedStartTime = startTime;

            Schedule(() => Process(vehicle), delay);
        }

        private void Process(Vehicle vehicle)
        {
            OnProcess?.Invoke(this, new VehicleProcessedEventArgs(vehicle, null));
            Schedule(() => Depart(vehicle), _processDuration);
        }

        private void Depart(Vehicle vehicle)
        {
            OnDepart?.Invoke(this, new VehicleDepartedEventArgs(vehicle, null));
        }
    }

    class PmPathSingleCapacity : Sandbox
    {
        public event EventHandler<VehicleArrivedEventArgs>? OnArrive;
        public event EventHandler<VehicleProcessedEventArgs>? OnProcess;
        public event EventHandler<VehicleDepartedEventArgs>? OnDepart;

        // Private Fields
        private TimeSpan _nextAvailableTime = TimeSpan.Zero;
        private readonly int _capacity;

        // Public Properties
        public int Capacity => _capacity;

        public PmPathSingleCapacity(string id, int seed, int capacity = 1) : base(id, seed)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive");

            _capacity = capacity;
        }

        public void RequestToEnter(Vehicle vehicle, TimeSpan timestamp)
        {
            Schedule(() => Arrive(vehicle), timestamp);
        }

        private void Arrive(Vehicle vehicle)
        {
            var processDuration = TimeSpan.FromSeconds(10);
            OnArrive?.Invoke(this, new VehicleArrivedEventArgs(vehicle, null));

            // Compute earliest start time respecting single-capacity processing
            var startTime = ClockTime < _nextAvailableTime ? _nextAvailableTime : ClockTime;
            var delay = startTime - ClockTime; // relative delay from now

            // Reserve capacity for this job so subsequent arrivals see the updated availability
            _nextAvailableTime = startTime + processDuration;

            Schedule(() => Process(vehicle), delay);
        }

        private void Process(Vehicle vehicle)
        {
            var processDuration = TimeSpan.FromSeconds(10);
            OnProcess?.Invoke(this, new VehicleProcessedEventArgs(vehicle, null));
            Schedule(() => Depart(vehicle), processDuration);
        }

        private void Depart(Vehicle vehicle)
        {
            OnDepart?.Invoke(this, new VehicleDepartedEventArgs(vehicle, null));
        }
    }
}