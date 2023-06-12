using NTorSpectator.Observer.Services;

namespace NTorSpectator.Observer.Mastodon;

public interface IReporter
{
    Task PublishReport(IReadOnlyCollection<TorWatchResults> watchResults);
}