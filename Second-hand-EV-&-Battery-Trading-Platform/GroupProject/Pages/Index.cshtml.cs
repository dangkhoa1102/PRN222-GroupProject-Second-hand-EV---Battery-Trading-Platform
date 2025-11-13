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
    public int TotalVehicleCount { get; private set; }
    public int TotalBatteryCount { get; private set; }
    public string HomePageUrl { get; private set; } = "/";

    public async Task<IActionResult> OnGetAsync()
    {
        // Trang công khai - không yêu cầu login
        // Lấy tất cả tin đã duyệt
        var allVehicles = await _vehicleListingService.GetApprovedListingsAsync();
        TotalVehicleCount = allVehicles.Count;
        // Chỉ hiển thị 8 tin mới nhất trên homepage
        ApprovedVehicles = allVehicles.Take(8).ToList();

        var allBatteries = await _batteryListingService.GetApprovedListingsAsync();
        TotalBatteryCount = allBatteries.Count;
        // Chỉ hiển thị 8 tin mới nhất trên homepage
        ApprovedBatteries = allBatteries.Take(8).ToList();

        return Page();
    }
}
