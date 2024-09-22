using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        var date = DateTime.Today; // Example: Get today's UMMs
        var umms = await _ummService.GetProductionUnavailabilityUMMsAsync(date);
        var rssMessages = await _rssService.GetRSSUMMFeedAsync();

        // Combine UMMs and RSS messages
        var combinedData = umms.Concat(rssMessages).ToList();

        // Pass data to the view
        ViewBag.CapacityData = combinedData;
        return View();
    }
}
