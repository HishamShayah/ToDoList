using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Application.Services;
using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

public class InvitationServiceTests
{
    private readonly Mock<IInvitationRepository> _mockRepo;
    private readonly InvitationService _service;
    private readonly Mock<ILogger<InvitationService>> _mockLogger;
    public InvitationServiceTests()
    {
        _mockRepo = new Mock<IInvitationRepository>();

        _mockLogger = new Mock<ILogger<InvitationService>>();
        _service = new InvitationService(_mockRepo.Object, _mockLogger.Object);

    }

    [Fact]
    public async Task InviteAsync_ShouldCreateInvitationAndReturnToken()
    {
        // Arrange
        var dto = new InviteUserDto
        {
            Email = "test@example.com",
            Role = "Guest"
        };
        var invitedByUserId = "user123";
        Invitation savedInvitation = null;

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Invitation>()))
                 .Callback<Invitation>(inv => savedInvitation = inv)
                 .Returns(Task.CompletedTask);

        // Act
        var token = await _service.InviteAsync(dto, invitedByUserId);

        // Assert
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Invitation>()), Times.Once);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.NotNull(savedInvitation);
        Assert.Equal(dto.Email, savedInvitation.Email);
        Assert.Equal(dto.Role, savedInvitation.Role);
        Assert.Equal(invitedByUserId, savedInvitation.InvitedByUserId);
        Assert.Equal(token, savedInvitation.Token);
        Assert.True(savedInvitation.ExpiryDate > DateTime.UtcNow);
    }
}
