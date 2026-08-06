namespace ServiceHub_IT.DTOs;

public sealed class SupabaseConnectionStatus
{
    public bool IsConfigured { get; init; }
    public bool IsConnected { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Error { get; init; }
}
