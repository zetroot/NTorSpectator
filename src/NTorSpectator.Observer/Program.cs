using Microsoft.Extensions.Options;
using NTorSpectator.Database;
using NTorSpectator.Observer.HealthChecks;
using NTorSpectator.Observer.Mastodon;
using NTorSpectator.Observer.Services;
using NTorSpectator.Observer.TorIntegration;
using NTorSpectator.Services;
using Prometheus;
using Quartz;
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
    .AddBizLogic()
    .AddDatabase(builder.Configuration)
    .AddTransient<TorControlManager>()
    .AddTransient<SpectatorJob>()
    .AddHostedService<SitesUpdater>();

builder.Services.AddQuartz(cfg =>
{
    cfg.UseMicrosoftDependencyInjectionJobFactory();
    
    var jobDetail = JobBuilder.Create<SpectatorJob>()
        .WithDescription("Tor spectator job")
        .WithIdentity("tor-spectator")
        .DisallowConcurrentExecution()
        .Build();
    cfg.AddJob<SpectatorJob>(jobKey: jobDetail.Key, configure: j => {});
    cfg.AddTrigger(t => t.WithCronSchedule("0 0 * * * ?").ForJob(jobDetail));
});
builder.Services.AddQuartzServer(cfg => cfg.WaitForJobsToComplete = false);

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
