using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Net.Http.Headers;
using System.Text;


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

            // Configure XmlReaderSettings to allow DTD processing
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse
            };

            using (var stringReader = new System.IO.StringReader(response))
            using (var xmlReader = XmlReader.Create(stringReader, settings))
            {
                var syndicationFeed = SyndicationFeed.Load(xmlReader);
                var ummMessages = new List<RSSMessage>();

                foreach (var item in syndicationFeed.Items)
                {
                    // Assuming that "ProductionUnavailability" is part of the title or description
                    if (item.Title.Text.Contains("ProductionUnavailability"))
                    {
                        ummMessages.Add(new RSSMessage
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

public class RSSMessage
{
    public string MessageType { get; set; } // Message type, like "ProductionUnavailability"
    public string ProductionType { get; set; } // Production type as a string
    public int UnavailableCapacity { get; set; } // Unavailable capacity in MW as an integer
}
