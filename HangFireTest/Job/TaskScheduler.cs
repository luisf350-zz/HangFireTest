using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace HangFireTest.Job
{
    public class TaskScheduler : ITaskScheduler
    {
        public TaskScheduler(IConfiguration configuration)
        {
            // Clean Queues when deploy
            ClearRecurringJobs();

            // List of Queues 
            //AddOrUpdateJob(() => , configuration.GetValue<string>("Hangfire:Jobs:UOM"));
        }

        #region Private Methods

        /// <summary>
        /// Add Or Update Job
        /// </summary>
        /// <param name="methodToCall"></param>
        /// <param name="queueName"></param>
        private void AddOrUpdateJob(Expression<Action> methodToCall, string queueName)
        {
            RecurringJob.AddOrUpdate(
                methodToCall,
                Cron.Minutely,
                null,
                queueName);
        }

        /// <summary>
        /// Clear previous jobs
        /// </summary>
        private void ClearRecurringJobs()
        {
            try
            {
                using IStorageConnection connection = JobStorage.Current.GetConnection();
                foreach (RecurringJobDto recurringJob in connection.GetRecurringJobs())
                {
                    if (recurringJob != null)
                    {
                        RecurringJob.RemoveIfExists(recurringJob.Id);
                    }
                }

                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();
                while (monitoringApi.Servers().Count > 1)
                {
                    var serverToRemove = monitoringApi.Servers().Last();
                    connection.RemoveServer(serverToRemove.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}
