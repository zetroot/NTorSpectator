using Microsoft.Extensions.Options;
using NTorSpectator.HealthChecks;
using NTorSpectator.Mastodon;
using NTorSpectator.Services;
using NTorSpectator.TorIntegration;
using Prometheus;
using Refit;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .Configure<TorSettings>(builder.Configuration.GetSection(nameof(TorSettings)))
    .Configure<SpectatorSettings>(builder.Configuration.GetSection(nameof(SpectatorSettings)))
    .Configure<MastodonSettings>(builder.Configuration.GetSection(nameof(MastodonSettings)));


builder.Services.RegisterApplicationHealthChecks(builder.Configuration);

builder.Services
    .AddTransient<IReporter, Reporter>()
    .AddTransient<TorControlManager>()
    .AddHostedService<Spectator>();

builder.Services
    .AddRefitClient<IMastodonClient>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MastodonSettings>>();
        return new RefitSettings { AuthorizationHeaderValueGetter = () => Task.FromResult(settings.Value.Token) };
    })
    .ConfigureHttpClient((sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<MastodonSettings>>();
        client.BaseAddress = settings.Value.Instance;
    });

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
