namespace NTorSpectator.Observer.TorIntegration;

public enum HsFailReason
{
    None = 0,
    BadDesc,
    QueryRejected,
    UploadRejected,
    NotFound,
    Unexpected,
    QueryNoHsDir,
    QueryRateLimited
}