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
    private readonly TokenService _tokenService;

    public UMMService(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<List<UMMMessage>> GetProductionUnavailabilityUMMsAsync(DateTime date, string username, string password)
    {
        // Get the access token
        var token = await _tokenService.GetAccessTokenAsync(username, password);

        // Add the Bearer token to the request headers
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Make the API request
        string url = $"https://umm.nordpoolgroup.com/api/messages?date={date:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error fetching data from API: {response.StatusCode} - {errorContent}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        // Deserialize the response
        var umms = JsonConvert.DeserializeObject<List<UMMMessage>>(responseBody);

        return umms.FindAll(umm => umm.MessageType == "ProductionUnavailability");
    }
}


public class UMMMessage
{
    public string MessageType { get; set; }
    public string ProductionType { get; set; }
    public string Area { get; set; }
    public int UnavailableCapacity { get; set; }
    public DateTime EventStart { get; set; }
    public DateTime EventEnd { get; set; }
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
        string rssFeedUrl = "https://umm.nordpoolgroup.com/#/messages?publicationDate=all&eventDate=nextweek";
        var response = await _httpClient.GetStringAsync(rssFeedUrl);

        using (var stringReader = new StringReader(response))
        using (var xmlReader = XmlReader.Create(stringReader))
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
                        MessageType = "ProductionUnavailability",
                        ProductionType = item.Title.Text,
                        UnavailableCapacity = int.Parse(item.Summary.Text) // Assuming unavailable capacity is in summary
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
