using Application.Messages.Commands;
using Application.Interfaces;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Infrastructure.Messaging;

/// <summary>
/// Azure Service Bus implementation of IServiceBusService.
/// </summary>
public class AzureServiceBusService : IServiceBusService, IAsyncDisposable
{
    private readonly ServiceBusProcessor _processor;
    private readonly IMediator _mediator;
    private readonly ILogger<AzureServiceBusService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AzureServiceBusService(
        IConfiguration configuration,
        IMediator mediator,
        ILogger<AzureServiceBusService> logger)
    {
        _mediator = mediator;
        _logger = logger;

        var connectionString = configuration["AzureServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("AzureServiceBus:ConnectionString is not configured.");
        var queueName = configuration["AzureServiceBus:QueueName"]
            ?? throw new InvalidOperationException("AzureServiceBus:QueueName is not configured.");

        var client = new ServiceBusClient(connectionString);
        _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += OnMessageReceivedAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        // Polly: 1 retry with 2-second delay
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 1,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(2),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    _logger.LogWarning(exception,
                        "Retry {RetryCount} after {Delay}s due to: {Message}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _processor.StartProcessingAsync(cancellationToken);
            _logger.LogInformation("Azure Service Bus processor started.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Service Bus processor.");
            throw;
        }
    }

    public async Task StopProcessingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _processor.StopProcessingAsync(cancellationToken);
            _logger.LogInformation("Azure Service Bus processor stopped.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Service Bus processor.");
            throw;
        }
    }

    private async Task OnMessageReceivedAsync(ProcessMessageEventArgs args)
    {
        var messageId = args.Message.MessageId;
        var correlationId = args.Message.CorrelationId;

        _logger.LogInformation(
            "Received Service Bus message {MessageId} | CorrelationId: {CorrelationId}",
            messageId, correlationId);

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var body = args.Message.Body.ToString();
                var subject = args.Message.Subject ?? "Unknown";

                var command = new ProcessServiceBusMessageCommand(
                    messageId, body, subject, correlationId);

                var result = await _mediator.Send(command, args.CancellationToken);

                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(
                        $"Command failed: {result.Error}");
                }
            });

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            _logger.LogInformation("Message {MessageId} completed.", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process message {MessageId} after retries. Dead-lettering.",
                messageId);

            await args.DeadLetterMessageAsync(
                args.Message,
                deadLetterReason: "ProcessingFailed",
                deadLetterErrorDescription: ex.Message,
                cancellationToken: args.CancellationToken);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Service Bus error on {EntityPath}: {ErrorSource}",
            args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
    }
}
