using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static UMMWebSocketService;

public class HomeController : Controller
{
    private readonly UMMWebSocketService _ummWebSocketService;

    public HomeController()
    {
        _ummWebSocketService = new UMMWebSocketService();
    }

    public IActionResult Index()
    {
        return View();
    }

    // This method is called to start listening to the WebSocket
    public async Task StartListening()
    {
        await _ummWebSocketService.ConnectToUMMWebSocketAsync(OnNewMessageReceived);
    }

    // This callback is triggered whenever a new ProductionUnavailability message is received
    private void OnNewMessageReceived(UMMMessage message)
    {
        // Process the message or send it to the front-end via SignalR, etc.
        Console.WriteLine($"New message received: {message.ProductionType} - {message.UnavailableCapacity} MW");
        // You can store messages and send them to the front-end via SignalR or another mechanism
    }

    // Call this method to stop the connection when necessary
    public async Task StopListening()
    {
        await _ummWebSocketService.DisconnectAsync();
    }
}
