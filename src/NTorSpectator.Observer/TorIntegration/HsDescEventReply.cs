namespace NTorSpectator.Observer.TorIntegration;

public record HsDescEventReply(int Code, string Message, HsDescAction Action, string HsAddress, string AuthType, string HsDir, string? DescriptorId, HsFailReason? Reason, string? Replica, string? HsDirIndex ) : TorReply(Code, Message);