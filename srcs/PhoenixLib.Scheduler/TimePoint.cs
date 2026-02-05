using System;

namespace PhoenixLib.Scheduler
{
    /// <summary>
    ///     Represents a given moment.
    /// </summary>
    public class TimePoint
    {
        /// <summary>
        ///     The hour value of the given moment.
        /// </summary>
        public byte Hour { get; private set; }

        /// <summary>
        ///     The minute value of the given moment.
        /// </summary>
        public byte Minute { get; private set; }

        /// <summary>
        ///     Specifies the hour of the given moment.
        /// </summary>
        /// <param name="hour">The hour between 0 and 23</param>
        /// <returns>A reference to the used TimePoint object.</returns>
        public TimePoint AtHour(byte hour)
        {
            if (Hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hour), "Value must be between 0 and 23.");
            }

            Hour = hour;

            return this;
        }

        /// <summary>
        ///     Specifies the minute of the given moment.
        /// </summary>
        /// <param name="minute">The minute between 0 and 59</param>
        /// <returns>A reference to the used TimePoint object.</returns>
        public TimePoint AtMinute(byte minute)
        {
            if (Minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute), "Value must be between 0 and 59");
            }

            Minute = minute;

            return this;
        }
    }
}