namespace NTorSpectator.Services;

public class Spectator : BackgroundService
{
    public bool IsRunning { get; private set; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            IsRunning = true;
            await Task.Delay(TimeSpan.Zero, stoppingToken);
        }
        finally
        {
            IsRunning = false;
        }
    }
}
