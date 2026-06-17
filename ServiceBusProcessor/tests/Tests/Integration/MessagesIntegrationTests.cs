using Application;
using Application.Messages.Commands;
using Application.Messages.Queries;
using Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Tests.Integration;

public class MessagesIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private ApplicationDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                   .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

        services.AddScoped<IServiceBusMessageRepository, ServiceBusMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddApplication();
        services.AddLogging(b => b.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task ProcessAndRetrieve_EndToEnd_ShouldSucceed()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var processCommand = new ProcessServiceBusMessageCommand(
            "integration-001", "Integration test body", "TestSubject", "corr-int-001");

        // Act — Process
        var processResult = await mediator.Send(processCommand);

        // Assert — process
        processResult.IsSuccess.Should().BeTrue();
        processResult.Value!.MessageId.Should().Be("integration-001");

        // Act — Retrieve by ID
        var getResult = await mediator.Send(
            new GetMessageByIdQuery(processResult.Value.Id));

        // Assert — retrieve
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value!.Subject.Should().Be("TestSubject");
        getResult.Value.CorrelationId.Should().Be("corr-int-001");
    }

    [Fact]
    public async Task ProcessDuplicate_ShouldReturnExisting_NotThrow()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new ProcessServiceBusMessageCommand(
            "dup-001", "Body", "Subject", null);

        // Act
        var first = await mediator.Send(command);
        var second = await mediator.Send(command);

        // Assert
        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Value!.Id.Should().Be(first.Value!.Id);
    }
}
