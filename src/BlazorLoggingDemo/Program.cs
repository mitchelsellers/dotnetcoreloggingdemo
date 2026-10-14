using BlazorLoggingDemo.Components;
using BlazorLoggingDemo.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var demoActivitySource = new System.Diagnostics.ActivitySource("BlazorLoggingDemo.DemoEndpoints");

builder.AddServiceDefaults();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.OpenTelemetry(options =>
    {
        // Aspire automatically injects the correct OTLP endpoint via environment variables
        options.Endpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]; 
        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;

        // Map Serilog properties correctly to OpenTelemetry attributes
        options.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = context.Configuration["OTEL_SERVICE_NAME"] ?? "UnknownService"
        };
    })
    .Enrich.FromLogContext());

builder.Services.AddHttpClient<DemoOutboundClient>(client =>
{
    var baseUrl = builder.Configuration["DemoOutbound:BaseUrl"] ?? "https://www.microsoft.com";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

var demoGroup = app.MapGroup("/demo").WithTags("Observability Demo");

demoGroup.MapGet("/success", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("DemoEndpoints");
    using var activity = demoActivitySource.StartActivity("demo.success", System.Diagnostics.ActivityKind.Server);

    var eventId = Guid.NewGuid().ToString("n");
    logger.LogInformation("Demo success endpoint executed for {EventId}", eventId);

    return Results.Ok(new
    {
        Message = "Success event emitted",
        EventId = eventId,
        TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
    });
});

demoGroup.MapGet("/warning", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("DemoEndpoints");
    using var activity = demoActivitySource.StartActivity("demo.warning", System.Diagnostics.ActivityKind.Server);

    var eventId = Guid.NewGuid().ToString("n");
    logger.LogWarning("Demo warning endpoint executed for {EventId}", eventId);

    return Results.Ok(new
    {
        Message = "Warning event emitted",
        EventId = eventId,
        TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
    });
});

demoGroup.MapGet("/error", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("DemoEndpoints");
    using var activity = demoActivitySource.StartActivity("demo.error", System.Diagnostics.ActivityKind.Server);

    var eventId = Guid.NewGuid().ToString("n");
    logger.LogError("Demo error endpoint about to throw for {EventId}", eventId);
    throw new InvalidOperationException($"Intentional demo exception for {eventId}");
});

demoGroup.MapGet("/outbound", async (DemoOutboundClient outboundClient, CancellationToken cancellationToken) =>
{
    var result = await outboundClient.CallSampleDependencyAsync(cancellationToken);
    return Results.Ok(result);
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
