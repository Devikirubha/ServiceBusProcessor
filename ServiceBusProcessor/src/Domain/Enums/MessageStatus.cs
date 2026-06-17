namespace Domain.Enums;

public enum MessageStatus
{
    Received = 0,
    Processing = 1,
    Processed = 2,
    Retrying = 3,
    Failed = 4,
    DeadLettered = 5
}
