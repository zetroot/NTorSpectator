using System.Net.Sockets;
using System.Text;
using NeoSmart.AsyncLock;

namespace NTorSpectator.TorIntegration;

public class TorControlManager
{
    private const string controlSocketName = "/run/tor/control"; 
    private Socket _torControl;
    private readonly ILogger<TorControlManager> _logger;

    private readonly object _connectSync = new();
    private readonly AsyncLock _commandSync = new();
    private readonly Memory<byte> rcvBuf = new byte[100_000];
    
    public TorControlManager(ILogger<TorControlManager> logger)
    {
        _logger = logger;
        _torControl = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
    }

    public void EnsureConnected()
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if (_torControl.Connected) return;
        lock (_connectSync)
        {
            if (!_torControl.Connected)
            {
                _logger.LogDebug("Not connected to tor control socket. Connecting now");
                _torControl = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                _torControl.Connect(new UnixDomainSocketEndPoint(controlSocketName));
                _logger.LogInformation("Connected to tor control socket!");
            }
        }
    }

    private async Task<TorReply[]> SendCommand(string commandText)
    {
        using var _ = await _commandSync.LockAsync(); 
        if (!commandText.EndsWith("\r\n"))
        {
            commandText = commandText + "\r\n";
        }
        _logger.LogTrace("Sending command {Command}", commandText.Trim());
        var command = Encoding.ASCII.GetBytes(commandText);
        await _torControl.SendAsync(command);
        _logger.LogDebug("Sent command to tor");

        var response = await Receive();
        return response.Select(x => TorReply.TryParse(x, out var parsed)? parsed : null).Where(x => x != null).ToArray()!;
    }

    public async Task<bool> Authenticate()
    {
        var authCookie = ToHex(File.ReadAllBytes("/run/tor/control.authcookie"));
        _logger.LogDebug("Read auth cookie");
        _logger.LogTrace("Auth cookie today is {Cookie}", authCookie);
        var response = await SendCommand($"AUTHENTICATE {authCookie}\r\n");
        _logger.LogTrace("Got auth response {@Response}", response);
        return response.Any(x => x == TorReply.Ok);
    }

    public async Task<IReadOnlyCollection<HsDescEventReply>> HsFetch(string serviceDesc)
    {
        EnsureConnected();
        await Authenticate();
        await SendCommand("SETEVENTS HS_DESC");
        await SendCommand($"HSFETCH {serviceDesc}");
        var results = new List<HsDescEventReply>(10);
        while (true)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            try
            {
                var responses = await Receive(cts.Token);
                _logger.LogTrace("Received {Count} messages", responses.Length);

                var replies = responses.Select(x => TorReply.TryParse(x, out var reply) ? reply : null).Where(x => x != null).ToList();
                _logger.LogInformation("Parsed {Count}", replies.Count);
                var hsDescEvents = replies.OfType<HsDescEventReply>().Where(x => x.Action is HsDescAction.Received or HsDescAction.Failed).ToList();
                results.AddRange(hsDescEvents);
                if (results.Any(x => x.Action == HsDescAction.Received))
                    break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return results;
    }

    private async Task<string[]> Receive(CancellationToken ct = default)
    {
        var rcvCnt = await _torControl.ReceiveAsync(rcvBuf, ct);
        var message = Encoding.ASCII.GetString(rcvBuf[..rcvCnt].Span);
        _logger.LogTrace("Received message: {Message}", message);
        return message.Split("\r\n", StringSplitOptions.TrimEntries| StringSplitOptions.RemoveEmptyEntries);
    }

    static string ToHex(params byte[] bytes)
    {
 
        var result = new StringBuilder(bytes.Length * 2);
        var hexAlphabet = "0123456789ABCDEF";

        foreach (byte b in bytes)
        {
            result.Append(hexAlphabet[b >> 4]);
            result.Append(hexAlphabet[b & 0xF]);
        }

        return result.ToString();
    }
}
