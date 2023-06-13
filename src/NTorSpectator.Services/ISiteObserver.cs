namespace NTorSpectator.Services;

public interface ISiteObserver
{
    Task AddNewObservation(string siteUri, bool isAvailable);
}
