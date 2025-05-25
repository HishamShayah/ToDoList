using Application.DTOs;
using Application.Interfaces;
using Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Owner,Guest")]
        public async Task<ActionResult<TaskDto>> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpGet("GetTasks")]
        [Authorize(Roles = "Owner,Guest")]
        public async Task<IActionResult> GetTasks([FromQuery] TaskSearchCriteria filter)
        {
            var tasks = await _taskService.GetFilteredTasksAsync(filter);
            return Ok(tasks);
        }

        [HttpPost]
       [Authorize(Roles = "Owner")]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto taskDto)
        {
            var task = await _taskService.CreateTaskAsync(taskDto);

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
       [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] CreateTaskDto taskDto)
        {
            try
            {
                await _taskService.UpdateTaskAsync(id, taskDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
      [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                await _taskService.DeleteTaskAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/toggle-completion")]
        [Authorize(Roles = "Owner,Guest")]
        public async Task<IActionResult> ToggleCompletion(int id)
        {
          
            var result = await _taskService.ToggleTaskCompletionAsync(id);

            if (!result)
                return NotFound(new { message = "Task not found or access denied." });

            return Ok(new { message = "Task completion status toggled successfully." });
        }
    }
}


