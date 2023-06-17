namespace NTorSpectator.Observer.Services;

public record TorWatchResults(string Site, int Positive, int Negative)
{
    public bool IsOk => Positive > 0;
}