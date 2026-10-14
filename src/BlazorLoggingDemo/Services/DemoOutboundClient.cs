namespace BlazorLoggingDemo.Services;

public sealed class DemoOutboundClient(HttpClient httpClient, ILogger<DemoOutboundClient> logger)
{
    public async Task<DemoOutboundResult> CallSampleDependencyAsync(CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid().ToString("n");

        logger.LogInformation("Outbound dependency call starting for {RequestId} to {Url}", requestId, httpClient.BaseAddress);

        using var response = await httpClient.GetAsync("/", cancellationToken);

        logger.LogInformation("Outbound dependency call completed for {RequestId} with status {StatusCode}", requestId, (int)response.StatusCode);

        return new DemoOutboundResult
        {
            RequestId = requestId,
            Target = httpClient.BaseAddress?.ToString() ?? "(not configured)",
            StatusCode = (int)response.StatusCode,
            IsSuccess = response.IsSuccessStatusCode
        };
    }
}

public sealed class DemoOutboundResult
{
    public string RequestId { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public bool IsSuccess { get; init; }
}
