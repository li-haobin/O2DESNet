using System;

namespace O2DESNet
{
    public interface IHourCounter : IReadOnlyHourCounter
    {
        /// <summary>
        /// Observes the count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="clockTime">The clock time.</param>
        void ObserveCount(double count, DateTime clockTime);

        /// <summary>
        /// Observes the change.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="clockTime">The clock time.</param>
        void ObserveChange(double count, DateTime clockTime);

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        void Pause();

        /// <summary>
        /// Pauses the specified clock time.
        /// </summary>
        /// <param name="clockTime">The clock time.</param>
        void Pause(DateTime clockTime);

        /// <summary>
        /// Resumes the specified clock time.
        /// </summary>
        /// <param name="clockTime">The clock time.</param>
        void Resume(DateTime clockTime);
    }
}