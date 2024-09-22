using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public class UMMWebSocketService
{
    private HubConnection _connection;

    // This method connects to the WebSocket using SSL (wss)
    public async Task ConnectToUMMWebSocketAsync(Action<UMMMessage> onMessageReceived)
    {
        // Secure WebSocket URL (replace with the actual WebSocket URL)
        string webSocketUrl = "wss://nordpool.com/websocket-endpoint";  // Replace with the actual URL

        // Create the connection using SignalR
        _connection = new HubConnectionBuilder()
            .WithUrl(webSocketUrl)  // Use the secure WebSocket URL
            .Build();

        // Handle incoming messages
        _connection.On<string>("ReceiveMessage", (messageJson) =>
        {
            // Deserialize the incoming message
            var message = JsonConvert.DeserializeObject<UMMMessage>(messageJson);

            // Filter by "ProductionUnavailability" messageType
            if (message?.MessageType == "ProductionUnavailability")
            {
                onMessageReceived?.Invoke(message);  // Invoke the callback function with the message
            }
        });

        // Try to start the connection
        try
        {
            await _connection.StartAsync();
            Console.WriteLine("Connected to WebSocket using SSL (wss).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to WebSocket: {ex.Message}");
        }
    }

    // Method to stop and dispose of the WebSocket connection
    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}

// The UMMMessage class represents the structure of the incoming message
public class UMMMessage
{
    public string MessageType { get; set; }  // E.g., "ProductionUnavailability"
    public string ProductionType { get; set; }
    public string Area { get; set; }
    public int UnavailableCapacity { get; set; }
    public DateTime EventStart { get; set; }
    public DateTime EventEnd { get; set; }
}
