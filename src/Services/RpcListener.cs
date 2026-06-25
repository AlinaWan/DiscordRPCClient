using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DiscordRPC.Models;

namespace DiscordRPC.Services
{
    public class RpcListener
    {
        private UdpClient? _udpListener;
        private CancellationTokenSource? _cts;
        private readonly Action<RpcProfile> _onProfileReceived;
        private readonly Action<string, bool> _onStatusReported; // text, isError

        public RpcListener(Action<RpcProfile> onProfileReceived, Action<string, bool> onStatusReported)
        {
            _onProfileReceived = onProfileReceived;
            _onStatusReported = onStatusReported;
        }

        public void Start(int port)
        {
            Stop(); // Ensure any previous listener is dead

            _cts = new CancellationTokenSource();
            _udpListener = new UdpClient(port);

            Task.Run(() => ListenLoop(_cts.Token), _cts.Token);
            _onStatusReported($"UDP Listener started on port {port}", false);
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Wait for incoming network packets
                    var result = await _udpListener!.ReceiveAsync(token);
                    string rawJson = Encoding.UTF8.GetString(result.Buffer);

                    // Attempt validation and deserialization
                    var updatedProfile = JsonSerializer.Deserialize<RpcProfile>(rawJson);

                    if (updatedProfile != null && !string.IsNullOrWhiteSpace(updatedProfile.ClientId))
                    {
                        // Safely pass valid update back to UI thread
                        _onProfileReceived(updatedProfile);
                    }
                    else
                    {
                        _onStatusReported("Port Update Rejected: Missing Client ID", true);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (JsonException)
                {
                    // This explicitly catches malformed JSON updates, ignoring them to avoid killing the listener
                    _onStatusReported("Port Update Rejected: Malformed JSON", true);
                }
                catch (Exception ex)
                {
                    _onStatusReported($"Port Error: {ex.Message}", true);
                }
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _udpListener?.Close();
            _udpListener = null;
            _cts = null;
        }
    }
}
