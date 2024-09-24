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
        List<string> wsMessages = new List<string>();

        DateTime startDate = DateTime.UtcNow.AddDays(-7);  // 7 days ago
        DateTime endDate = DateTime.UtcNow;  // Current date

        try
        {
            // Fetch data from the UMM API with startDate and endDate
            umms = await _ummService.GetProductionUnavailabilityUMMsAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            ViewBag.UMMErrorMessage = $"UMM API failed: {ex.Message}";
        }

        try
        {
            // Fetch data from the RSS feed (this should always run, regardless of UMM API status)
            rssMessages = await _rssService.GetRSSUMMFeedAsync();
        }
        catch (Exception ex)
        {
            ViewBag.RSSErrorMessage = $"RSS feed failed: {ex.Message}";
        }

        try
        {
            // Get real-time WebSocket messages from WSService
            wsMessages = _wsService.GetMessages();
        }
        catch (Exception ex)
        {
            ViewBag.WSErrorMessage = $"WebSocket service failed: {ex.Message}";
        }

        // Serialize UMM, RSS, and WebSocket data for the view
        ViewBag.UMMDataJson = JsonConvert.SerializeObject(umms);
        ViewBag.RSSDataJson = JsonConvert.SerializeObject(rssMessages);
        ViewBag.WSDataJson = JsonConvert.SerializeObject(wsMessages);

        return View();
    }

    public async Task<IActionResult> RefreshUMMData()
    {
        List<UMMMessage> umms = new List<UMMMessage>();
        DateTime startDate = DateTime.UtcNow.AddDays(-7);  // 7 days ago
        DateTime endDate = DateTime.UtcNow;  // Current date

        try
        {
            // Fetch UMM data again (refreshing)
            umms = await _ummService.GetProductionUnavailabilityUMMsAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            ViewBag.UMMErrorMessage = $"UMM API failed: {ex.Message}";
        }

        // Serialize the refreshed data and return a JSON result
        return Json(new { data = JsonConvert.SerializeObject(umms) });
    }
}
