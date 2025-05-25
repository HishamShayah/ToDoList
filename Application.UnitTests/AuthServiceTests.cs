using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Options;
using AutoMapper;
using Application.Services;
using Application.Interfaces;
using Application.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Core.Helpers;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly JWT _jwtSettings;
    private readonly AuthService _service;

    public AuthServiceTests()
    {

        _mockRepo = new Mock<IAuthRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _jwtSettings = new JWT
        {
            Key = "NfoEuuwmDuIug59L2aMgeNo3GhYZrIug59L2aM",
            Issuer = "TodoList",
            Audience = "TodoList",
            DurationInDays = 1
        };


        var jwtOptions = Options.Create(_jwtSettings);
        _service = new AuthService(_mockRepo.Object, jwtOptions, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var model = new TokenRequestModel { Username = "test", Password = "pass" };
        var user = new IdentityUser { UserName = "test", Email = "test@example.com", Id = "1" };
        var authDto = new AuthDto { Email = "test@example.com", UserName = "test" };

        _mockRepo.Setup(r => r.GetUserByUsernameAsync(model.Username)).ReturnsAsync(user);
        _mockRepo.Setup(r => r.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
        _mockRepo.Setup(r => r.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        _mockMapper.Setup(m => m.Map<AuthDto>(user)).Returns(authDto);

        // Act
        var result = await _service.GetTokenAsync(model);

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("test", result.UserName);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnError_WhenCredentialsAreInvalid()
    {
        var model = new TokenRequestModel { Username = "wrong", Password = "wrong" };

        _mockRepo.Setup(r => r.GetUserByUsernameAsync(model.Username)).ReturnsAsync((IdentityUser)null);

        var result = await _service.GetTokenAsync(model);

        Assert.False(result.IsAuthenticated);
        Assert.Equal("UserName or Password is incorrect!", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnToken_WhenRegistrationIsSuccessful()
    {
        var register = new RegisterDto { UserName = "new", Email = "new@example.com", Password = "Pass123!" };
        var user = new IdentityUser { UserName = "new", Email = "new@example.com", Id = "2" };
        var authDto = new AuthDto { Email = "new@example.com", UserName = "new" };

        _mockRepo.Setup(r => r.AddUserAsync(It.IsAny<IdentityUser>(), register.Password))
                 .ReturnsAsync(IdentityResult.Success);
        _mockRepo.Setup(r => r.GetRolesAsync(It.IsAny<IdentityUser>()))
                 .ReturnsAsync(new List<string> { "Guest" });
        _mockMapper.Setup(m => m.Map<AuthDto>(It.IsAny<IdentityUser>())).Returns(authDto);

        var result = await _service.RegisterAsync(register);

        Assert.True(result.IsAuthenticated);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnError_WhenRegistrationFails()
    {
        var register = new RegisterDto { UserName = "fail", Email = "fail@example.com", Password = "123" };
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Weak password" });

        _mockRepo.Setup(r => r.AddUserAsync(It.IsAny<IdentityUser>(), register.Password))
                 .ReturnsAsync(failedResult);

        var result = await _service.RegisterAsync(register);

        Assert.False(result.IsAuthenticated);
        Assert.Equal("Weak password", result.Message);
    }
}
