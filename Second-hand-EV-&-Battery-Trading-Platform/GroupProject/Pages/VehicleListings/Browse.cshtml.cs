using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GroupProject.Pages.VehicleListings;

public class BrowseModel : PageModel
{
    private readonly IVehicleListingService _vehicleListingService;

    public BrowseModel(IVehicleListingService vehicleListingService)
    {
        _vehicleListingService = vehicleListingService;
    }

    public List<VehicleListingDto> Listings { get; private set; } = new();
    public string? SearchKeyword { get; private set; }
    public string? SelectedBrand { get; private set; }
    public int TotalCount { get; private set; }
    public string CurrentPageUrl { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? search, string? brand)
    {
        // Trang công khai - không yêu cầu login
        var allListings = await _vehicleListingService.GetApprovedListingsAsync();

        // Filter theo search keyword
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            SearchKeyword = search;
            allListings = allListings.Where(v =>
                (v.Brand != null && v.Brand.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (v.Model != null && v.Model.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (v.Condition != null && v.Condition.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (v.BatteryCapacity != null && v.BatteryCapacity.Contains(search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Filter theo brand
        if (!string.IsNullOrWhiteSpace(brand))
        {
            SelectedBrand = brand.Trim();
            allListings = allListings.Where(v =>
                v.Brand != null && v.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        TotalCount = allListings.Count;
        Listings = allListings;

        // Lấy danh sách brands để hiển thị filter
        ViewData["Brands"] = await GetBrandsAsync();

        // Tạo URL hiện tại để truyền vào returnUrl
        CurrentPageUrl = Request.Path + Request.QueryString;

        return Page();
    }

    private async Task<List<SelectListItem>> GetBrandsAsync()
    {
        var allListings = await _vehicleListingService.GetApprovedListingsAsync();
        var brands = allListings
            .Where(v => !string.IsNullOrWhiteSpace(v.Brand))
            .Select(v => v.Brand!)
            .Distinct()
            .OrderBy(b => b)
            .Select(b => new SelectListItem { Text = b, Value = b })
            .ToList();

        return brands;
    }
}

