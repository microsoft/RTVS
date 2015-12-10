using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionExtensions {

        /// <summary>
        /// Schedules function in evaluation queue without waiting. 
        /// </summary>
        /// <param name="session">R Session</param>
        /// <param name="function">Function to scheduel</param>
        public static void ScheduleEvaluation(this IRSession session, Func<IRSessionEvaluation, Task> function) {
            session.GetScheduleEvaluationTask(function).DoNotWait();
        }

        private static async Task GetScheduleEvaluationTask(this IRSession session, Func<IRSessionEvaluation, Task> function) {
            TaskUtilities.AssertIsOnBackgroundThread();
            using (var evaluation = await session.BeginEvaluationAsync()) {
                await function(evaluation);
            } 
        }
    }
}
