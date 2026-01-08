using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Health checks для Docker
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => "Schvacheno Payments Service v1.0");

app.Run();
