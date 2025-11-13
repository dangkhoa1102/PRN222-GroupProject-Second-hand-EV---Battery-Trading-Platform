using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GroupProject.Pages.BatteryListings;

public class BrowseModel : PageModel
{
    private readonly IBatteryListingService _batteryListingService;

    public BrowseModel(IBatteryListingService batteryListingService)
    {
        _batteryListingService = batteryListingService;
    }

    public List<BatteryListingDto> Listings { get; private set; } = new();
    public string? SearchKeyword { get; private set; }
    public string? SelectedBrand { get; private set; }
    public int TotalCount { get; private set; }
    public string CurrentPageUrl { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? search, string? brand)
    {
        // Trang công khai - không yêu cầu login
        var allListings = await _batteryListingService.GetApprovedListingsAsync();

        // Filter theo search keyword
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            SearchKeyword = search;
            allListings = allListings.Where(b =>
                (b.Brand != null && b.Brand.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (b.BatteryType != null && b.BatteryType.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (b.Condition != null && b.Condition.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (b.Capacity != null && b.Capacity.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (b.Voltage != null && b.Voltage.Contains(search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Filter theo brand
        if (!string.IsNullOrWhiteSpace(brand))
        {
            SelectedBrand = brand.Trim();
            allListings = allListings.Where(b =>
                b.Brand != null && b.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase)
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
        var allListings = await _batteryListingService.GetApprovedListingsAsync();
        var brands = allListings
            .Where(b => !string.IsNullOrWhiteSpace(b.Brand))
            .Select(b => b.Brand!)
            .Distinct()
            .OrderBy(b => b)
            .Select(b => new SelectListItem { Text = b, Value = b })
            .ToList();

        return brands;
    }
}

