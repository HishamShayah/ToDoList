using Hangfire;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Application.BackgroundJobs;

namespace ToDoList.API.Controllers
{
    [ApiController]
    [Route("api/hangfire")]
    public class HangfireJobsController : ControllerBase
    {
        // Fire-and-forget
        [HttpPost("cleanup-now")]
        public IActionResult CleanupNow()
        {
            var jobId = BackgroundJob.Enqueue<ITodoJobs>(
                x => x.CleanupOldTasksAsync()
            );

            return Ok(new
            {
                Message = "Cleanup job queued successfully!",
                JobId = jobId
            });
        }

        // Delayed
        [HttpPost("cleanup-delayed")]
        public IActionResult CleanupDelayed()
        {
            var jobId = BackgroundJob.Schedule<ITodoJobs>(
                x => x.CleanupOldTasksAsync(),
                TimeSpan.FromMinutes(2)
            );

            return Ok(new
            {
                Message = "Cleanup job scheduled after 2 minutes.",
                JobId = jobId
            });
        }

        //  Continuation
        [HttpPost("cleanup-continuation")]
        public IActionResult CleanupWithContinuation()
        {
            // Job 1: Cleanup
            var cleanupJobId = BackgroundJob.Enqueue<ITodoJobs>(
                x => x.CleanupOldTasksAsync()
            );

            // Job 2: After summary => notify admin
            var notifyJobId = BackgroundJob.ContinueJobWith<ITodoJobs>(
                cleanupJobId,
                x => x.NotifyAdminAsync()
            );

            return Ok(new
            {
                Message = "Cleanup + Continuations scheduled successfully!",
                CleanupJobId = cleanupJobId,
                NotifyJobId = notifyJobId
            });
        }
    }
}
