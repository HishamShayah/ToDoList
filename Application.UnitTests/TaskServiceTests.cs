using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Core.Entities;
using System;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<TaskService>>();
        _service = new TaskService(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldAddTaskAndReturnDto()
    {
        var createDto = new CreateTaskDto { Title = "Test Task", Priority = 2 };
        var taskEntity = new TaskItem { Id = 1, Title = "Test Task", Priority = 2 };
        var taskDto = new TaskDto { Id = 1, Title = "Test Task", Priority = 2 };

        _mockMapper.Setup(m => m.Map<TaskItem>(createDto)).Returns(taskEntity);
        _mockRepo.Setup(r => r.AddTaskAsync(taskEntity)).Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<TaskDto>(taskEntity)).Returns(taskDto);

        var result = await _service.CreateTaskAsync(createDto);

        _mockRepo.Verify(r => r.AddTaskAsync(taskEntity), Times.Once);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnTask_WhenFound()
    {
        var task = new TaskItem { Id = 1, Title = "Task 1" };
        var taskDto = new TaskDto { Id = 1, Title = "Task 1" };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(1)).ReturnsAsync(task);
        _mockMapper.Setup(m => m.Map<TaskDto>(task)).Returns(taskDto);

        var result = await _service.GetTaskByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetTaskByIdAsync(999)).ReturnsAsync((TaskItem)null);

        var result = await _service.GetTaskByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldUpdate_WhenTaskExists()
    {
        var existing = new TaskItem { Id = 1, Title = "Old" };
        var updateDto = new CreateTaskDto { Title = "Updated", Priority = 3 };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.UpdateTaskAsync(existing)).Returns(Task.CompletedTask);

        await _service.UpdateTaskAsync(1, updateDto);

        _mockMapper.Verify(m => m.Map(updateDto, existing), Times.Once);
        _mockRepo.Verify(r => r.UpdateTaskAsync(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldThrow_WhenTaskNotFound()
    {
        _mockRepo.Setup(r => r.GetTaskByIdAsync(1)).ReturnsAsync((TaskItem)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateTaskAsync(1, new CreateTaskDto()));
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldDelete_WhenTaskExists()
    {
        var task = new TaskItem { Id = 1 };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(1)).ReturnsAsync(task);
        _mockRepo.Setup(r => r.DeleteTaskAsync(1)).Returns(Task.CompletedTask);

        await _service.DeleteTaskAsync(1);

        _mockRepo.Verify(r => r.DeleteTaskAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldThrow_WhenTaskNotFound()
    {
        _mockRepo.Setup(r => r.GetTaskByIdAsync(1)).ReturnsAsync((TaskItem)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteTaskAsync(1));
    }

    [Fact]
    public async Task ToggleTaskCompletionAsync_ShouldReturnExpectedResult()
    {
        _mockRepo.Setup(r => r.ToggleCompletionAsync(1)).ReturnsAsync(true);

        var result = await _service.ToggleTaskCompletionAsync(1);

        Assert.True(result);
        _mockRepo.Verify(r => r.ToggleCompletionAsync(1), Times.Once);
    }
}
