using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json; // Ensure Newtonsoft.Json is included for serialization

public class HomeController : Controller
{
    private readonly UMMService _ummService;
    private readonly RSSService _rssService;

    public HomeController(UMMService ummService, RSSService rssService)
    {
        _ummService = ummService;
        _rssService = rssService;
    }

    public async Task<IActionResult> Index()
    {
        List<UMMMessage> umms = new List<UMMMessage>();
        List<UMMMessage> rssMessages = new List<UMMMessage>();

        try
        {
            // Fetch data from the UMM API
            umms = await _ummService.GetProductionUnavailabilityUMMsAsync();
        }
        catch (Exception ex)
        {
            // Log the error and display a message (optional)
            ViewBag.UMMErrorMessage = $"UMM API failed: {ex.Message}";
        }
        /*
        try
        {
            // Fetch data from the RSS feed (this should always run, regardless of UMM API status)
            rssMessages = await _rssService.GetRSSUMMFeedAsync();
        }
        catch (Exception ex)
        {
            // Log the error and display a message if RSS feed also fails
            ViewBag.RSSErrorMessage = $"RSS feed failed: {ex.Message}";
        }
        */

        // Combine the data, if UMM API data exists it will be concatenated with RSS feed data
        var combinedData = umms.Concat(rssMessages).ToList();

        // Serialize the combined data for use in the view
        var capacityDataJson = JsonConvert.SerializeObject(combinedData);

        // Pass the serialized data to the view using ViewBag
        ViewBag.CapacityDataJson = capacityDataJson;

        return View();
    }
}
