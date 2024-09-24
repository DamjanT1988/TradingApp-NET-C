using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly UMMService _ummService;
    private readonly RSSService _rssService;
    private readonly WSService _wsService;

    public HomeController(UMMService ummService, RSSService rssService, WSService wsService)
    {
        _ummService = ummService;
        _rssService = rssService;
        _wsService = wsService;
    }

    public async Task<IActionResult> Index()
    {
        List<UMMMessage> umms = new List<UMMMessage>();
        List<RSSMessage> rssMessages = new List<RSSMessage>();

        // Set the date range for fetching UMM messages (example: last 7 days)
        DateTime startDate = DateTime.UtcNow.AddDays(-7);  // 7 days ago
        DateTime endDate = DateTime.UtcNow;  // Current date

        try
        {
            // Fetch data from the UMM API with startDate and endDate
            umms = await _ummService.GetProductionUnavailabilityUMMsAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            // Log the error and display a message (optional)
            ViewBag.UMMErrorMessage = $"UMM API failed: {ex.Message}";
        }

        try
        {
            // Fetch data from the RSS feed
            rssMessages = await _rssService.GetRSSUMMFeedAsync();
        }
        catch (Exception ex)
        {
            // Log the error and display a message if RSS feed also fails
            ViewBag.RSSErrorMessage = $"RSS feed failed: {ex.Message}";
        }

        // Start WebSocket listening
        await _wsService.StartListeningAsync();

        // Serialize the separate data for use in the view
        ViewBag.UMMDataJson = JsonConvert.SerializeObject(umms);
        ViewBag.RSSDataJson = JsonConvert.SerializeObject(rssMessages);
        ViewBag.WebSocketMessagesJson = JsonConvert.SerializeObject(_wsService.GetMessages());

        return View();
    }
}
