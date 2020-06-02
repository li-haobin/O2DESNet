using System;

namespace O2DESNet
{
    public interface IReadOnlyHourCounter
    {
        /// <summary>
        /// Gets the last time.
        /// </summary>
        DateTime LastTime { get; }

        /// <summary>
        /// Gets the last count.
        /// </summary>
        double LastCount { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IReadOnlyHourCounter"/> is paused.
        /// </summary>
        /// <value>
        ///   <c>true</c> if paused; otherwise, <c>false</c>.
        /// </value>
        bool Paused { get; }

        /// <summary>
        /// Total number of increment observed
        /// </summary>
        double TotalIncrement { get; }

        /// <summary>
        /// Total number of decrement observed
        /// </summary>
        double TotalDecrement { get; }

        /// <summary>
        /// Gets the increment rate.
        /// </summary>
        double IncrementRate { get; }

        /// <summary>
        /// Gets the decrement rate.
        /// </summary>
        double DecrementRate { get; }

        /// <summary>
        /// Total number of hours since the initial time.
        /// </summary>
        double TotalHours { get; }

        /// <summary>
        /// Gets the working time ratio.
        /// </summary>
        double WorkingTimeRatio { get; }

        /// <summary>
        /// The cumulative count value on time in unit of hours
        /// </summary>
        double CumValue { get; }

        /// <summary>
        /// The average count on observation period
        /// </summary>
        double AverageCount { get; }

        /// <summary>
        /// Average timespan that a load stays in the activity, if it is a stationary process, 
        /// i.e., decrement rate == increment rate
        /// It is 0 at the initial status, i.e., decrement rate is NaN (no decrement observed).
        /// </summary>
        TimeSpan AverageDuration { get; }

        /// <summary>
        /// Gets or sets the log file.
        /// </summary>
        string LogFile { get; set; }
    }
}