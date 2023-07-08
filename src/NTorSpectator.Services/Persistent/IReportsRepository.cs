using NTorSpectator.Services.Models;

namespace NTorSpectator.Services.Persistent;

/// <summary>
/// Reports repository
/// </summary>
public interface IReportsRepository
{
    /// <summary>
    /// Save report to db
    /// </summary>
    /// <param name="report">Report to save</param>
    Task SaveReport(Report report);
}
