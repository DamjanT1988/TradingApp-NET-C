using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WSService
{
    private const string ServerURI = "https://ummws.nordpoolgroup.com/"; // SignalR endpoint base URL
    private IHubProxy _hubProxy;
    private HubConnection _connection;
    public List<string> WebSocketMessages { get; private set; } = new List<string>();

    public WSService()
    {
        // Initialize the SignalR connection using the classic ASP.NET SignalR Client
        _connection = new HubConnection(ServerURI);
        _hubProxy = _connection.CreateHubProxy("MessageHub");

        // Subscribe to the events from the SignalR Hub
        _hubProxy.On("newMessage", message => HandleNewMessage(message));
        _hubProxy.On("updateMessage", message => HandleUpdateMessage(message));
        _hubProxy.On("dismissMessage", message => HandleDismissMessage(message));
    }

    // Method to start the SignalR connection and listen to events
    public async Task StartListeningAsync()
    {
        try
        {
            // Start the SignalR connection
            await _connection.Start();
            Console.WriteLine("Connected to the UMM WebSocket SignalR endpoint.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to the server: {ex.Message}");
        }
    }

    // Handlers for each SignalR event
    private void HandleNewMessage(dynamic message)
    {
        WebSocketMessages.Add($"New Message: {message}");
        Console.WriteLine($"New Message Received: {message}");
    }

    private void HandleUpdateMessage(dynamic message)
    {
        WebSocketMessages.Add($"Updated Message: {message}");
        Console.WriteLine($"Message Updated: {message}");
    }

    private void HandleDismissMessage(dynamic message)
    {
        WebSocketMessages.Add($"Dismissed Message: {message}");
        Console.WriteLine($"Message Dismissed: {message}");
    }

    public List<string> GetMessages()
    {
        return WebSocketMessages;
    }
}
