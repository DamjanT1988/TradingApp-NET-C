using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class WSService
{
    private readonly IHubContext<UMMHub> _hubContext;

    public WSService(IHubContext<UMMHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task ConnectToWebSocketAsync()
    {
        var uri = new Uri("wss://ummws.nordpoolgroup.com/signalr/signalr/connect?transport=webSockets&clientProtocol=2.1&connectionToken=hr2wx6x2kTtvtwNHCtvLdm5nCKIfyiWd8D+5QMicqTTTqTWEzOmqn6gnDTMkbllxSsJsnjy/Pp9wWJBGyGDkHmNdAvGuH7c0xemY8UGLoPgXkSgOXFMLEmhPzkn20X2l&connectionData=[{\"name\":\"messagehub\"}]&tid=4");

        using (var webSocket = new ClientWebSocket())
        {
            await webSocket.ConnectAsync(uri, CancellationToken.None);
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received Message: {message}");

                    // Broadcast message to connected clients via SignalR
                    await _hubContext.Clients.All.SendAsync("ReceiveUMMMessage", message);
                }
            }
        }
    }
}


public class UMMHub : Hub
{
}
