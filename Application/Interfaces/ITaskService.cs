using Application.DTOs;
using Core.Helpers;

namespace Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto);
        Task<TaskDto> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskDto>> GetFilteredTasksAsync(TaskSearchCriteria queryParams);
        Task UpdateTaskAsync(int id, CreateTaskDto taskDto);
        Task DeleteTaskAsync(int id);
        Task<bool> ToggleTaskCompletionAsync(int taskId);

    }
}
