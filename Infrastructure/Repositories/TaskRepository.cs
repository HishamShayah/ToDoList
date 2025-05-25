
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _db;

        public TaskRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            return await _db.Tasks.FindAsync(id);
        }

        public async Task<IEnumerable<TaskItem>> GetFilteredTasksAsync(TaskSearchCriteria criteria)
        {
            var query = _db.Tasks.AsQueryable();

            if (!string.IsNullOrEmpty(criteria.Title))
            {
                query = query.Where(t => t.Title.Contains(criteria.Title));
            }

            if (!string.IsNullOrEmpty(criteria.Category))
            {
                query = query.Where(t => t.Category.Contains(criteria.Category));
            }
            if (criteria.Priority.HasValue)
                query = query.Where(t => t.Priority == criteria.Priority.Value);

            query = query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize);

            return await query.ToListAsync();
        }



        public async Task AddTaskAsync(TaskItem task)
        {
            await _db.Tasks.AddAsync(task);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(TaskItem task)
        {
            _db.Tasks.Update(task);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task != null)
            {
                _db.Tasks.Remove(task);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> ToggleCompletionAsync(int taskId)
        {
            var task = await _db.Tasks.FindAsync(taskId);

            if (task == null)
                return false;

            task.Toggle();
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
