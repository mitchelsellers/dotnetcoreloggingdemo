# BlazorLoggingDemo Observability Guide

## Run with Aspire AppHost
1. Set `LoggingDemo.AppHost` as startup project.
2. Run the solution.
3. Open the Blazor app from the Aspire dashboard.

## Serilog Outputs
- Console logs appear in app output.
- Rolling file logs are written to `src/BlazorLoggingDemo/Logs/blazor-demo-*.txt`.

## Demo Scenarios
### Blazor UI
- Navigate to **Observability** and trigger:
  - Information log
  - Warning log
  - Handled exception log
  - Outbound dependency call log
- Navigate to **Counter** and click increment to generate activity + log events.
- Navigate to **Weather** to generate scoped load logs and activity tags.

### HTTP Endpoints
- `GET /demo/success`
- `GET /demo/warning`
- `GET /demo/error` (intentional exception)
- `GET /demo/outbound`

## Telemetry Expectations
- Request and dependency traces are captured by OpenTelemetry from `AddServiceDefaults()`.
- Custom spans are emitted from:
  - `BlazorLoggingDemo.Demos`
  - `BlazorLoggingDemo.DemoEndpoints`
- To export telemetry, set `OTEL_EXPORTER_OTLP_ENDPOINT` before running AppHost.
