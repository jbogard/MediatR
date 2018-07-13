using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines strategy for multiple  task execution
    /// </summary>
    public interface ITaskExecutionStrategy
    {
        /// <summary>
        /// Executes many tasks
        /// </summary>
        /// <param name="tasks">collection of tasks to be executed</param>
        /// <returns></returns>
        Task Execute(IEnumerable<Task> tasks);
    }

    /// <summary>
    /// Default task execution strategy using WhenAll
    /// </summary>
    public class DefaultExecutionStrategy : ITaskExecutionStrategy
    {
        public Task Execute(IEnumerable<Task> tasks) => Task.WhenAll(tasks);
    }

    /// <summary>
    /// Tasks are awaited one by one
    /// </summary>
    public class SequentialExecutionStrategy : ITaskExecutionStrategy
    {
        public async Task Execute(IEnumerable<Task> tasks)
        {
            foreach (var @task in tasks)
            {
                await @task.ConfigureAwait(false);
            }
        }
    }
}