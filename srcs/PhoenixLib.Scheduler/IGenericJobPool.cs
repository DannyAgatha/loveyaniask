using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PhoenixLib.Scheduler
{
    /// <summary>
    ///     This interface represents a task/job scheduler.
    /// </summary>
    /// <typeparam name="TId">Type of the id that will be used to distinguish every job.</typeparam>
    public interface IGenericJobPool<TId>
    {
        /// <summary>
        ///     Enqueues a new job.
        /// </summary>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId Enqueue(Expression<Action> method);

        /// <summary>
        ///     Enqueues a new job.
        /// </summary>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId Enqueue(Expression<Action<Task>> method);

        /// <summary>
        ///     Enqueues a new job.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId Enqueue<T>(Expression<Action<T>> method);

        /// <summary>
        ///     Enqueues a new job.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId Enqueue<T>(Expression<Action<Task<T>>> method);

        /// <summary>
        ///     Creates a new job that will be enqueued after a given delay.
        /// </summary>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <param name="delay">Delay before enqueuing the job.</param>
        /// <returns></returns>
        TId Schedule(Expression<Action> method, TimeSpan delay);

        /// <summary>
        ///     Creates a new job that will be enqueued after a given delay.
        /// </summary>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <param name="delay">Delay before enqueuing the job.</param>
        /// <returns></returns>
        TId Schedule(Expression<Action<Task>> method, TimeSpan delay);

        /// <summary>
        ///     Creates a new job that will be enqueued after a given delay.
        /// </summary>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <param name="delay">Delay before enqueuing the job.</param>
        /// <returns></returns>
        TId Schedule(Expression<Func<Task>> method, TimeSpan delay);

        /// <summary>
        ///     Creates a new job that will be enqueued after a given delay.
        /// </summary>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <param name="delay">Delay before enqueuing the job.</param>
        /// <returns></returns>
        TId Schedule<T>(Expression<Action<T>> method, TimeSpan delay);

        /// <summary>
        ///     Creates a new job that will be enqueued after a given delay.
        /// </summary>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <param name="delay">Delay before enqueuing the job.</param>
        /// <returns></returns>
        TId Schedule<T>(Expression<Action<Task<T>>> method, TimeSpan delay);


        /// <summary>
        ///     Creates a new job that will wait for parent job completion before to be enqueued.
        /// </summary>
        /// <param name="parentJobId">Job id of the parent.</param>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId ContinueWith(TId parentJobId, Expression<Action> method);

        /// <summary>
        ///     Creates a new job that will wait for parent job completion before to be enqueued.
        /// </summary>
        /// <param name="parentJobId">Job id of the parent.</param>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId ContinueWith(TId parentJobId, Expression<Action<Task>> method);

        /// <summary>
        ///     Creates a new job that will wait for parent job completion before to be enqueued.
        /// </summary>
        /// <param name="parentJobId">Job id of the parent.</param>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId ContinueWith<T>(TId parentJobId, Expression<Action<T>> method);

        /// <summary>
        ///     Creates a new job that will wait for parent job completion before to be enqueued.
        /// </summary>
        /// <param name="parentJobId">Job id of the parent.</param>
        /// <param name="method">Method to call when the job is executed.</param>
        /// <returns></returns>
        TId ContinueWith<T>(TId parentJobId, Expression<Action<Task<T>>> method);

        /// <summary>
        ///     Deletes an existing job.
        /// </summary>
        /// <param name="jobId">Id of the job.</param>
        void Remove(TId jobId);
    }
}