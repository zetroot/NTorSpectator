using NTorSpectator;
using NTorSpectator.HealthChecks;
using Prometheus;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
);

Serilog.Debugging.SelfLog.Enable(msg => Log.Logger.Error(msg));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AppOptions>(builder.Configuration);
builder.Services.RegisterApplicationHealthChecks(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new()
{
    Predicate = healthCheck => healthCheck.Tags.Contains("health")
});
app.MapHealthChecks("/ready", new()
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});
app.MapHealthChecks("/alive", new()
{
    Predicate = healthCheck => healthCheck.Tags.Contains("alive")
});
app.MapMetrics();
Metrics.SuppressDefaultMetrics();
app.Run();
