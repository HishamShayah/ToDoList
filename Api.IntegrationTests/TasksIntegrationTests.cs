using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Api;
using Application.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TasksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TasksIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GenerateTestJwtToken());

    }

    [Fact]
    public async Task CreateTask_ShouldReturnSuccess_AndSaveTask()
    {
        // Arrange
        var newTask = new CreateTaskDto
        {
            Title = "Integration Test Task",
            Description = "Created via integration test",
            Priority = 3,
            Category = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var content = await response.Content.ReadAsStringAsync();

        //  Assert
        Assert.True(response.IsSuccessStatusCode,
            $"Expected success but got {(int)response.StatusCode} {response.StatusCode}.\nResponse Body:\n{content}");

        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(result);
        Assert.Equal("Integration Test Task", result.Title);
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnCreatedTask()
    {
        var newTask = new CreateTaskDto
        {
            Title = "Test Get",
            Description = "Testing retrieval",
            Priority = 1,
            Category = "Test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        var fetchedTask = await getResponse.Content.ReadFromJsonAsync<TaskDto>();

        Assert.True(getResponse.IsSuccessStatusCode,
            $"Expected 200 OK but got {(int)getResponse.StatusCode} {getResponse.StatusCode}");

        Assert.NotNull(fetchedTask);
        Assert.Equal(createdTask.Id, fetchedTask.Id);
        Assert.Equal("Test Get", fetchedTask.Title);
    }

    [Fact]
    public async Task DeleteTask_ShouldRemoveTaskSuccessfully()
    {
        var newTask = new CreateTaskDto
        {
            Title = "Task to Delete",
            Description = "Will be deleted",
            Priority = 2,
            Category = "Test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        Assert.True(deleteResponse.IsSuccessStatusCode,
            $"Expected 200 OK on delete but got {(int)deleteResponse.StatusCode} {deleteResponse.StatusCode}");

        var getAfterDelete = await _client.GetAsync($"/api/tasks/{createdTask.Id}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
public async Task UpdateTask_ShouldModifyTaskSuccessfully()
{
    var originalTask = new CreateTaskDto
    {
        Title = "Original Task",
        Description = "Before update",
        Priority = 1,
        Category = "Initial"
    };

    var createResponse = await _client.PostAsJsonAsync("/api/tasks", originalTask);
    var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

    var updatedTask = new CreateTaskDto
    {
        Title = "Updated Task",
        Description = "After update",
        Priority = 5,
        Category = "Updated"
    };

    var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", updatedTask);

    Assert.True(updateResponse.IsSuccessStatusCode,
        $"Expected 200 OK on update but got {(int)updateResponse.StatusCode} {updateResponse.StatusCode}");

    var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
    var taskAfterUpdate = await getResponse.Content.ReadFromJsonAsync<TaskDto>();

    Assert.Equal("Updated Task", taskAfterUpdate.Title);
    Assert.Equal("After update", taskAfterUpdate.Description);
    Assert.Equal(5, taskAfterUpdate.Priority);
    Assert.Equal("Updated", taskAfterUpdate.Category);
}


    private string GenerateTestJwtToken()
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, "owner@elkood.com"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, "owner@elkood.com"),
        new Claim("uid", "fb36be05-cad5-4001-8711-83f6bb70fe1d"),
        new Claim(ClaimTypes.Role, "Owner")
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("2aMgeNo3GhYZrkNfoEuuwmDuIug59LUnFPKfrjDU3hk="));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TodoList",
            audience: "TodoList",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
