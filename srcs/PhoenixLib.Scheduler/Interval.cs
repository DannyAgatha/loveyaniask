using System;

namespace PhoenixLib.Scheduler
{
    /// <summary>
    ///     This class wraps a time interval.
    ///     You can use this class to process some recurring operations with a delay.
    ///     The delay can be represented by this class.
    /// </summary>
    public class Interval
    {
        /// <summary>
        ///     Minute's value.
        /// </summary>
        public byte Minute { get; private set; }

        /// <summary>
        ///     Hour's value.
        /// </summary>
        public byte Hour { get; private set; }

        /// <summary>
        ///     Specifies how many minute is required by the interval to be completed.
        /// </summary>
        /// <param name="minute">An integer withing [0; 59] range</param>
        /// <returns></returns>
        public Interval EveryMinute(byte minute)
        {
            if (minute > 59)
            {
                throw new InvalidOperationException("Minute should be between 0~59.");
            }

            Minute = minute;
            return this;
        }


        /// <summary>
        ///     Specifies how many hours is required by the interval to be completed.
        /// </summary>
        /// <param name="hour">An integer withing [0; 23] range</param>
        /// <returns></returns>
        public Interval EveryHour(byte hour)
        {
            if (hour > 23)
            {
                throw new InvalidOperationException("Hour should be between 0~23.");
            }

            Hour = hour;
            return this;
        }
    }
}