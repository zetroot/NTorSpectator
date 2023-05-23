namespace NTorSpectator.TorIntegration;

public record TorReply(int Code, string Message)
{
    public static TorReply Ok { get; } = new(250, "OK");

    public static bool TryParse(string message, out TorReply result)
    {
        if (message.Length < 3)
        {
            result = new(0, message);
            return false;
        }
        
        var codeToken = message[..3];
        if (!int.TryParse(codeToken, out var code))
        {
            result = new(0, message);
            return false;
        }

        if (code == 250)
        {
            result = Ok;
            return true;
        }

        if (code != 650)
        {
            result = new TorReply(code, message);
            return true;
        }

        var messageTokens = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (messageTokens[1] != "HS_DESC")
        {
            result = new TorReply(code, message);
            return true;
        }

        if (messageTokens.Length < 6)
        {
            result = new TorReply(0, message);
            return false;
        }
            
        var action = ParseAction(messageTokens[2]);
        var hsAddr = messageTokens[3];
        var authType = messageTokens[4];
        var hsDir = messageTokens[5];
        string? descriptorId = null, replica = null, hsDirIndex = null;
        HsFailReason? reason = null;
        for (int i = 6; i < messageTokens.Length; ++i)
        {
            if (messageTokens[i].StartsWith("REASON="))
                reason = ParseFailReason(messageTokens[i]);
            else if (messageTokens[i].StartsWith("REPLICA="))
                replica = messageTokens[i].Replace("REPLICA=", string.Empty);
            else if (messageTokens[i].StartsWith("HSDIR_INDEX="))
                hsDirIndex = messageTokens[i].Replace("HSDIR_INDEX=", string.Empty);
            else
                descriptorId = messageTokens[i];
        }

        result = new HsDescEventReply(code, message, action, hsAddr, authType, hsDir, descriptorId, reason, replica, hsDirIndex);
        return true;
    }

    private static HsFailReason ParseFailReason(string failReason)
    {
        if (failReason.StartsWith("REASON=", StringComparison.OrdinalIgnoreCase))
            failReason = failReason.Replace("REASON=", string.Empty, StringComparison.OrdinalIgnoreCase);
        return failReason.Trim().ToUpperInvariant() switch
        {
            "BAD_DESC" => HsFailReason.BadDesc,
            "QUERY_REJECTED" => HsFailReason.QueryRejected,
            "UPLOAD_REJECTED" => HsFailReason.UploadRejected,
            "NOT_FOUND" => HsFailReason.NotFound,
            "UNEXPECTED" => HsFailReason.Unexpected,
            "QUERY_NO_HSDIR" => HsFailReason.QueryNoHsDir,
            "QUERY_RATE_LIMITED" => HsFailReason.QueryRateLimited,
            _ => HsFailReason.None
        };
    }
    
    private static HsDescAction ParseAction(string action)
    {
        return action.Trim().ToUpperInvariant() switch
        {
            "REQUESTED" => HsDescAction.Requested,
            "UPLOAD" => HsDescAction.Upload,
            "RECEIVED" => HsDescAction.Received,
            "UPLOADED" => HsDescAction.Uploaded,
            "IGNORE" => HsDescAction.Ignore,
            "FAILED" => HsDescAction.Failed,
            "CREATED" => HsDescAction.Created,
            _ => HsDescAction.None
        };
    }
};
