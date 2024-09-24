using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;
using System.ServiceModel.Syndication;
using HtmlAgilityPack;

public class RSSService
{
    private readonly HttpClient _httpClient;

    public RSSService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RSSMessage>> GetRSSUMMFeedAsync()
    {
        string rssFeedUrl = "https://ummrss.nordpoolgroup.com/messages/?publicationStartDate=2024-06-30T23%3A00%3A00.000Z&eventStartDate=2024-09-22T22%3A00%3A00.000Z&eventStopDate=2024-09-29T21%3A59%3A59.999Z&limit=100";
        var response = await _httpClient.GetStringAsync(rssFeedUrl);

        Console.WriteLine("Response from RSS Feed: " + response); // Log the raw response

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse
        };

        using (var stringReader = new System.IO.StringReader(response))
        using (var xmlReader = XmlReader.Create(stringReader, settings))
        {
            // Define the XML namespace used in the feed
            var atomNamespace = "http://www.w3.org/2005/Atom";
            var syndicationFeed = SyndicationFeed.Load(xmlReader);
            var ummMessages = new List<RSSMessage>();

            foreach (var item in syndicationFeed.Items)
            {
                var title = item.Title.Text;
                var contentHtml = item.Content?.ToString();

                if (!string.IsNullOrEmpty(contentHtml))
                {
                    // Parse the HTML content inside <content>
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(contentHtml);

                    // Extract unavailable capacity and production type
                    var unavailableCapacity = ExtractUnavailableCapacity(htmlDoc);
                    var productionType = ExtractProductionType(htmlDoc);

                    if (productionType != null && unavailableCapacity.HasValue)
                    {
                        ummMessages.Add(new RSSMessage
                        {
                            MessageType = title,
                            ProductionType = productionType,
                            UnavailableCapacity = unavailableCapacity.Value
                        });

                        Console.WriteLine($"Added Message: ProductionType={productionType}, UnavailableCapacity={unavailableCapacity}");
                    }
                }
            }

            Console.WriteLine($"Total Messages Parsed: {ummMessages.Count}");
            return ummMessages;
        }
    }

    private int? ExtractUnavailableCapacity(HtmlDocument htmlDoc)
    {
        // Find the row that contains 'Unavailable Capacity' and extract the following cell value
        var unavailableCapacityNode = htmlDoc.DocumentNode.SelectSingleNode("//td[contains(text(), 'Unavailable Capacity')]/following-sibling::td");
        if (unavailableCapacityNode != null)
        {
            var capacityText = unavailableCapacityNode.InnerText.Replace(" MW", "").Trim();
            if (int.TryParse(capacityText, out var capacity))
            {
                return capacity;
            }
        }
        return null;
    }

    private string ExtractProductionType(HtmlDocument htmlDoc)
    {
        // Find the row that contains 'Fuel Type' and extract the following cell value
        var fuelTypeNode = htmlDoc.DocumentNode.SelectSingleNode("//td[contains(text(), 'Fuel Type')]/following-sibling::td");
        return fuelTypeNode?.InnerText.Trim();
    }
}

public class RSSMessage
{
    public string MessageType { get; set; } // Message type, like "ProductionUnavailability"
    public string ProductionType { get; set; } // Production type as a string
    public int UnavailableCapacity { get; set; } // Unavailable capacity in MW as an integer
}
