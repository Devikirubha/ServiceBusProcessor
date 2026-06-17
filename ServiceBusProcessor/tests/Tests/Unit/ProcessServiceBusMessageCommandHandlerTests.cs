using Application.Messages.Commands;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Unit;

public class ProcessServiceBusMessageCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IServiceBusMessageRepository> _repoMock;
    private readonly Mock<ILogger<ProcessServiceBusMessageCommandHandler>> _loggerMock;
    private readonly ProcessServiceBusMessageCommandHandler _handler;

    public ProcessServiceBusMessageCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repoMock = new Mock<IServiceBusMessageRepository>();
        _loggerMock = new Mock<ILogger<ProcessServiceBusMessageCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.ServiceBusMessages).Returns(_repoMock.Object);

        _handler = new ProcessServiceBusMessageCommandHandler(
            _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NewMessage_ShouldSaveAndReturnSuccess()
    {
        // Arrange
        var command = new ProcessServiceBusMessageCommand(
            "msg-001", "Hello World", "OrderCreated", "corr-001");

        _repoMock.Setup(r => r.GetByMessageIdAsync("msg-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceBusMessage?)null);

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MessageId.Should().Be("msg-001");
        result.Value.Subject.Should().Be("OrderCreated");

        _repoMock.Verify(r => r.AddAsync(
            It.IsAny<ServiceBusMessage>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u =>
            u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateMessage_ShouldReturnExistingRecord()
    {
        // Arrange
        var existing = ServiceBusMessage.Create("msg-002", "Body", "Subject");
        existing.MarkAsProcessed();

        var command = new ProcessServiceBusMessageCommand(
            "msg-002", "Body", "Subject", null);

        _repoMock.Setup(r => r.GetByMessageIdAsync("msg-002", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.MessageId.Should().Be("msg-002");

        _repoMock.Verify(r => r.AddAsync(
            It.IsAny<ServiceBusMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ShouldRollbackAndReturnFailure()
    {
        // Arrange
        var command = new ProcessServiceBusMessageCommand(
            "msg-003", "Body", "Subject", null);

        _repoMock.Setup(r => r.GetByMessageIdAsync("msg-003", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceBusMessage?)null);

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("DB error");

        _unitOfWorkMock.Verify(u =>
            u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
