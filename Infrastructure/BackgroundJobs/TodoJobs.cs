using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ToDoList.Application.BackgroundJobs;

namespace ToDoList.Infrastructure.BackgroundJobs
{
    public class TodoJobs : ITodoJobs
    {
        private readonly ApplicationDbContext _db; 

        public TodoJobs(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CleanupOldTasksAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var oldCompleted = await _db.Tasks
             .Where(x => x.CreatedAt < cutoff)
             .ToListAsync();
                if (oldCompleted.Any())
                {
                    _db.Tasks.RemoveRange(oldCompleted);
                    await _db.SaveChangesAsync();
                }
                Console.WriteLine($"[Hangfire] Cleanup done. Removed: {oldCompleted.Count}");

        }

        public Task SendDailySummaryAsync()
        {
            Console.WriteLine($"[Hangfire] Daily summary triggered at: {DateTime.UtcNow}");
            return Task.CompletedTask;
        }

        public Task NotifyAdminAsync()
        {
            Console.WriteLine("[Hangfire] NOTIFY: Admin notified after cleanup.");
            return Task.CompletedTask;
        }
    }
}
