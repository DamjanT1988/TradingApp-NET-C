using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        List<RSSMessage> rssMessages = new List<RSSMessage>();

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

        // Serialize the separate data for use in the view
        ViewBag.UMMDataJson = JsonConvert.SerializeObject(umms);
        ViewBag.RSSDataJson = JsonConvert.SerializeObject(rssMessages);

        return View();
    }
}
