using AutoMapper;
using Application.DTOs;
using Core.Entities;
using Application.Interfaces;
using Core.Interfaces;
using System.Threading.Tasks;
using Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;
        public TaskService(ITaskRepository taskRepository, IMapper mapper, ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto)
        {
            _logger.LogInformation("Creating new task: {Title}", taskDto.Title);

            var taskItem = _mapper.Map<TaskItem>(taskDto);
            await _taskRepository.AddTaskAsync(taskItem);

            _logger.LogInformation("Task created with ID: {Id}", taskItem.Id);

            return _mapper.Map<TaskDto>(taskItem);
        }

        public async Task<TaskDto> GetTaskByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving task with ID: {Id}", id);

            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {Id} not found", id);

                return null;
            }
            _logger.LogInformation("Task retrieved: {Title}", task.Title);

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<IEnumerable<TaskDto>> GetFilteredTasksAsync(TaskSearchCriteria filterDto)
        {
            _logger.LogInformation("Fetching tasks with filter: {@Filter}", filterDto);

            var criteria = _mapper.Map<TaskSearchCriteria>(filterDto);
            var tasks = await _taskRepository.GetFilteredTasksAsync(criteria);

            _logger.LogInformation("Retrieved {Count} tasks matching filter", tasks.Count());

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }


        public async Task UpdateTaskAsync(int id, CreateTaskDto taskDto)
        {
            _logger.LogInformation("Updating task with ID: {Id}", id);

            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task == null)
            {
                _logger.LogError("Task with ID: {Id} not found for update", id);
                throw new KeyNotFoundException($"Task with id {id} not found.");
            }

            _mapper.Map(taskDto, task);

            await _taskRepository.UpdateTaskAsync(task);
            _logger.LogInformation("Task updated successfully: {Id}", id);

        }
        public async Task DeleteTaskAsync(int id)
        {
            _logger.LogInformation("Deleting task with ID: {Id}", id);

            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task == null)
            {
                _logger.LogError("Task with ID: {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Task with id {id} not found.");
            }

            await _taskRepository.DeleteTaskAsync(id);
            _logger.LogInformation("Task deleted successfully: {Id}", id);

        }

        public async Task<bool> ToggleTaskCompletionAsync(int taskId)
        {
            _logger.LogInformation("Toggling completion status for task ID: {Id}", taskId);

            var result = await _taskRepository.ToggleCompletionAsync(taskId);

            if (result)
                _logger.LogInformation("Task completion toggled: {Id}", taskId);
            else
                _logger.LogWarning("Toggling failed or task not found: {Id}", taskId);

            return result;
        }

    }
}

