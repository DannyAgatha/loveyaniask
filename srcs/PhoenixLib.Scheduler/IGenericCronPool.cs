using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PhoenixLib.Scheduler
{
    /// <summary>
    ///     This interface represents a cron scheduler.
    ///     A cron is a recurring job/task.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public interface IGenericCronPool<in TId>
    {
        /// <summary>
        ///     Creates a new cron.
        /// </summary>
        /// <param name="method">Method to call when the cron is executed.</param>
        /// <param name="interval">Time interval between each cron execution.</param>
        /// <returns></returns>
        void New(TId cronId, Expression<Action> method, Interval interval);

        /// <summary>
        ///     Creates a new cron.
        /// </summary>
        /// <param name="method">Method to call when the cron is executed.</param>
        /// <param name="interval">Time interval between each cron execution.</param>
        /// <returns></returns>
        void New(TId cronId, Expression<Action<Task>> method, Interval interval);


        /// <summary>
        ///     Creates a new cron.
        /// </summary>
        /// <param name="method">Method to call when the cron is executed.</param>
        /// <param name="interval">Time interval between each cron execution.</param>
        /// <returns></returns>
        void New<T>(TId cronId, Expression<Action<T>> method, Interval interval);


        /// <summary>
        ///     Creates a new cron.
        /// </summary>
        /// <param name="method">Method to call when the cron is executed.</param>
        /// <param name="interval">Time interval between each cron execution.</param>
        /// <returns></returns>
        void New<T>(TId cronId, Expression<Action<Task<T>>> method, Interval interval);

        /// <summary>
        ///     Removes by id an existing cron.
        ///     May throw if the given cron id does not correspond to a valid cron.
        /// </summary>
        /// <param name="cronId">Id of the cron to remove.</param>
        void Remove(TId cronId);

        /// <summary>
        ///     Behaves similarly as 'Remove' but with a strictly defined
        ///     behavior and a performance penalty is the price to pay:
        ///     it checks if the given cron id is valid before trying to remove
        ///     the corresponding cron.
        /// </summary>
        /// <param name="cronId"></param>
        void RemoveIfExists(TId cronId);
    }
}