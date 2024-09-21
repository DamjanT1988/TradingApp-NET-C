using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;
using System;

public class UMMService
{
    private readonly HttpClient _httpClient;
    private HubConnection _connection;

    public UMMService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Method to retrieve UMMs filtered by ProductionUnavailability for a specific date
    public async Task<List<UMMMessage>> GetProductionUnavailabilityUMMsAsync(DateTime date)
    {
        string url = $"https://umm.nordpoolgroup.com/api/messages?date={date:yyyy-MM-dd}";
        var response = await _httpClient.GetStringAsync(url);

        // Deserialize and filter by messageType
        var umms = JsonConvert.DeserializeObject<List<UMMMessage>>(response);
        return umms.Where(umm => umm.MessageType == "ProductionUnavailability").ToList();
    }

    // Method to connect to UMM WebSocket using SignalR and listen for real-time updates
    public async Task ConnectToUMMWebSocketAsync(Action<UMMMessage> onMessageReceived)
    {
        // Ensure connection is not already started
        if (_connection == null)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://umm.nordpoolgroup.com/hub")  // Adjust this URL if necessary
                .Build();

            _connection.On<string>("ReceiveMessage", (messageJson) =>
            {
                var message = JsonConvert.DeserializeObject<UMMMessage>(messageJson);
                if (message?.MessageType == "ProductionUnavailability")
                {
                    onMessageReceived?.Invoke(message);
                }
            });

            // Start connection with basic error handling
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("Connected to WebSocket.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to WebSocket: {ex.Message}");
                // Handle reconnection or retries if needed
            }
        }
    }

    // Method to stop the WebSocket connection
    public async Task DisconnectFromUMMWebSocketAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}

// UMMMessage class to represent the message structure
public class UMMMessage
{
    public string MessageType { get; set; }
    public string ProductionType { get; set; }
    public string Area { get; set; }
    public int UnavailableCapacity { get; set; }
    // Add other fields as necessary
}
