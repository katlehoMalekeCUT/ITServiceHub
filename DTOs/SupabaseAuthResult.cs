namespace ServiceHub_IT.DTOs;

public sealed class SupabaseAuthResult
{
    public bool IsSuccess { get; init; }
    public string? AccessToken { get; init; }
    public string? ErrorMessage { get; init; }
    public string? RawResponse { get; init; }
}
