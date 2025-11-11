using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IVehicleListingService _vehicleListingService;
    private readonly IBatteryListingService _batteryListingService;

    public IndexModel(
        ILogger<IndexModel> logger,
        IVehicleListingService vehicleListingService,
        IBatteryListingService batteryListingService)
    {
        _logger = logger;
        _vehicleListingService = vehicleListingService;
        _batteryListingService = batteryListingService;
    }

    public List<VehicleListingDto> ApprovedVehicles { get; private set; } = new();
    public List<BatteryListingDto> ApprovedBatteries { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!HttpContext.Session.GetInt32("UserId").HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        ApprovedVehicles = await _vehicleListingService.GetApprovedListingsAsync();
        ApprovedBatteries = await _batteryListingService.GetApprovedListingsAsync();

        return Page();
    }
}
