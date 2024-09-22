using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using System.ServiceModel.Syndication;

public class UMMService
{
    private readonly HttpClient _httpClient;

    public UMMService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UMMMessage>> GetProductionUnavailabilityUMMsAsync(DateTime date)
    {
        string url = $"https://umm.nordpoolgroup.com/api/messages?date={date:yyyy-MM-dd}";
        var response = await _httpClient.GetStringAsync(url);

        // Deserialize the response
        var umms = JsonConvert.DeserializeObject<List<UMMMessage>>(response);

        // Filter for "ProductionUnavailability"
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
        string rssFeedUrl = "https://your-nordpool-rss-feed-url";
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
