using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Application.BackgroundJobs
{
    public interface ITodoJobs
    {
        Task CleanupOldTasksAsync();
        Task SendDailySummaryAsync();
        Task NotifyAdminAsync();
    }
}

