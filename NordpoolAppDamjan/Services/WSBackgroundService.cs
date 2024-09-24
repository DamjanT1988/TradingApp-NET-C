using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;

public class WSBackgroundService : IHostedService
{
    private readonly WSService _wsService;

    public WSBackgroundService(WSService wsService)
    {
        _wsService = wsService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _wsService.ConnectToWebSocketAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
