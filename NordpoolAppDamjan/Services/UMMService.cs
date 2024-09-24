using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public class UMMService
{
    private readonly HttpClient _httpClient;

    public UMMService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UMMMessage>> GetProductionUnavailabilityUMMsAsync(
        DateTime startDate, DateTime endDate,
        int skip = 0, int limit = 100,
        string status = "Active",
        string order = "PublicationDate",
        string orderDirection = "DESC")
    {
        // Format dates as ISO8601 (UTC)
        string formattedStartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        string formattedEndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Prepare the query parameters
        var queryParams = new Dictionary<string, string>
        {
            { "messageTypes", "ProductionUnavailability" }, // Filter for ProductionUnavailability
            { "status", status }, // Filter by active status
            { "publicationStartDate", formattedStartDate },
            { "publicationStopDate", formattedEndDate },
            { "eventStartDate", formattedStartDate },
            { "eventStopDate", formattedEndDate },
            { "skip", skip.ToString() }, // Paging support: skip records
            { "limit", limit.ToString() }, // Limit the results to prevent exceeding the limit
            { "order", order }, // Order by publication date
            { "orderDirection", orderDirection } // Order direction (ascending or descending)
        };

        // Build the query string from the dictionary
        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));

        // Build the complete URL
        string url = $"https://ummapi.nordpoolgroup.com/messages?{queryString}";

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

        // Return the list of messages
        return ummResponse.Items;
    }
}

public class UMMResponse
{
    [JsonProperty("items")]
    public List<UMMMessage> Items { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }
}

public class UMMMessage
{
    [JsonProperty("messageType")]
    public string MessageType { get; set; } // Message type (e.g., "ProductionUnavailability")

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
    public string FuelType { get; set; } // Assuming FuelType is a string (e.g., "FossilGas")

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
