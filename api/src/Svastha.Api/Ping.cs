namespace Svastha.Api;

public record Ping(long Id, string Message, DateTimeOffset CreatedAt);

public record CreatePingRequest(string Message);

public static class PingValidation
{
    public static bool IsValid(CreatePingRequest request) =>
        !string.IsNullOrWhiteSpace(request.Message);
}
