namespace NTorSpectator.Services;

public interface IReporter
{
    Task ReportWentDown(string siteUri, DateTime lastSeen);
    Task ReportCameUp(string siteUri);
}
