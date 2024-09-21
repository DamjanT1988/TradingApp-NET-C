using Microsoft.AspNetCore.Mvc;
using NordpoolAppDamjan.Models;
using System.Diagnostics;

namespace NordpoolAppDamjan.Controllers
{
    public class HomeController : Controller
    {
        private readonly UMMService _ummService;

        public HomeController(UMMService ummService)
        {
            _ummService = ummService;
        }

        public async Task<IActionResult> Index()
        {
            // Get UMM data for production unavailability
            var umms = await _ummService.GetProductionUnavailabilityUMMsAsync(DateTime.Today);

            // Group by ProductionType and calculate total unavailable capacity
            var capacityByType = umms.GroupBy(umm => umm.ProductionType)
                                     .Select(g => new
                                     {
                                         ProductionType = g.Key,
                                         TotalCapacity = g.Sum(umm => umm.UnavailableCapacity),
                                         Area = g.First().Area // Assuming each UMMMessage has an Area field
                                     }).ToList();

            // Convert the data to JSON in the controller
            ViewBag.CapacityDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(capacityByType);

            return View();
        }

    }


}