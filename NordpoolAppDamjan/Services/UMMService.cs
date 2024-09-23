using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Net.Http.Headers;
using System.Text;


public class UMMService
{
    private readonly HttpClient _httpClient;

    public UMMService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UMMMessage>> GetProductionUnavailabilityUMMsAsync()
    {
        // Get current UTC date and time for API request
        var now = DateTime.UtcNow;
        var startDate = now.AddDays(-7); // Example: 7 days ago
        var endDate = now; // Now

        // Format the start and end dates as ISO8601 (UTC)
        string formattedStartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        string formattedEndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Build the URL using the formatted dates
        string url = $"https://ummapi.nordpoolgroup.com/messages?publicationStartDate={formattedStartDate}&eventStartDate={formattedStartDate}&eventStopDate={formattedEndDate}";

        // Make the GET request to the API
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error fetching data from API: {response.StatusCode} - {errorContent}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        // Deserialize the response to UMMResponse (which contains the items list)
        var ummResponse = JsonConvert.DeserializeObject<UMMResponse>(responseBody);

        if (ummResponse == null || ummResponse.Items == null)
        {
            return new List<UMMMessage>(); // Return an empty list if response is null
        }

        // Check that MessageType is an integer (1 = "ProductionUnavailability") 
        return ummResponse.Items.FindAll(umm => umm.MessageType == 1);  // Assuming '1' is the correct message type for "ProductionUnavailability"

    }
}

public class UMMResponse
{
    [JsonProperty("items")]
    public List<UMMMessage> Items { get; set; }
}

public class UMMMessage
{
    [JsonProperty("messageType")]
    public int MessageType { get; set; } // Make sure this is the correct type (assuming it's an int)

    [JsonProperty("unavailabilityReason")]
    public string UnavailabilityReason { get; set; }

    [JsonProperty("remarks")]
    public string Remarks { get; set; }

    [JsonProperty("productionUnits")]
    public List<ProductionUnit> ProductionUnits { get; set; }

    [JsonProperty("marketParticipants")]
    public List<MarketParticipant> MarketParticipants { get; set; }

    [JsonProperty("publicationDate")]
    public DateTime PublicationDate { get; set; }

    [JsonProperty("eventStart")]
    public DateTime EventStart { get; set; }

    [JsonProperty("eventEnd")]
    public DateTime EventEnd { get; set; }
}

public class ProductionUnit
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("eic")]
    public string Eic { get; set; }

    [JsonProperty("fuelType")]
    public int FuelType { get; set; }

    [JsonProperty("areaEic")]
    public string AreaEic { get; set; }

    [JsonProperty("areaName")]
    public string AreaName { get; set; }

    [JsonProperty("installedCapacity")]
    public int InstalledCapacity { get; set; }

    [JsonProperty("timePeriods")]
    public List<TimePeriod> TimePeriods { get; set; }
}

public class TimePeriod
{
    [JsonProperty("unavailableCapacity")]
    public int UnavailableCapacity { get; set; }

    [JsonProperty("availableCapacity")]
    public int AvailableCapacity { get; set; }

    [JsonProperty("eventStart")]
    public DateTime EventStart { get; set; }

    [JsonProperty("eventStop")]
    public DateTime EventStop { get; set; }
}

public class MarketParticipant
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("acerCode")]
    public string AcerCode { get; set; }

    [JsonProperty("eicCode")]
    public string EicCode { get; set; }

    [JsonProperty("leiCode")]
    public string LeiCode { get; set; }
}







public class RSSService
{
    private readonly HttpClient _httpClient;

    public RSSService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UMMMessage>> GetRSSUMMFeedAsync()
    {
        string rssFeedUrl = "https://ummrss.nordpoolgroup.com/messages/?publicationStartDate=2023-12-31T23%3A00%3A00.000Z&eventStartDate=2024-09-22T22%3A00%3A00.000Z&eventStopDate=2024-09-29T21%3A59%3A59.999Z&limit=100";
        var response = await _httpClient.GetStringAsync(rssFeedUrl);

        // Configure XmlReaderSettings to allow DTD processing
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse
        };

        using (var stringReader = new System.IO.StringReader(response))
        using (var xmlReader = XmlReader.Create(stringReader, settings))
        {
            var syndicationFeed = SyndicationFeed.Load(xmlReader);
            var ummMessages = new List<UMMMessage>();

            foreach (var item in syndicationFeed.Items)
            {
                // Assuming that "ProductionUnavailability" is part of the title or description
                if (item.Title.Text.Contains("ProductionUnavailability"))
                {
                    ummMessages.Add(new UMMMessage
                    {
                        //MessageType = "ProductionUnavailability",
                        //ProductionType = item.Title.Text,
                        //UnavailableCapacity = int.Parse(item.Summary.Text) // Assuming unavailable capacity is in summary
                    });
                }
            }

            return ummMessages;
        }
    }
}

public class TokenService
{
    private readonly HttpClient _httpClient;

    public TokenService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetAccessTokenAsync(string username, string password)
    {
        var tokenUrl = "https://sts.nordpoolgroup.com/connect/token"; // Token URL for production

        var clientId = "client_remit_api";
        var clientSecret = "client_remit_api"; // As per the documentation, this is also the client secret

        // Base64 encode clientId:clientSecret
        var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        // Prepare the request headers
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

        // Prepare the form data
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("scope", "global"), // Change scope if needed
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        });

        // Send the request
        var response = await _httpClient.PostAsync(tokenUrl, formData);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Token request failed: {response.StatusCode} - {error}");
        }

        // Get the token response
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

        // Return the access token
        return tokenResponse.AccessToken;
    }
}

// Token response class to handle the token
public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }
}
