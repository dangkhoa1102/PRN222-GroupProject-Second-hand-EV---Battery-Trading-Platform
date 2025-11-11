using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.BatteryListings;

public class DetailsModel : PageModel
{
    private readonly IBatteryListingService _batteryListingService;

    public DetailsModel(IBatteryListingService batteryListingService)
    {
        _batteryListingService = batteryListingService;
    }

    public BatteryListingDetailDto? Listing { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsOwner { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var listing = await _batteryListingService.GetListingDetailAsync(id);
        if (listing is null)
        {
            ErrorMessage = "Tin đăng không tồn tại.";
            return Page();
        }

        IsOwner = listing.SellerId == userId.Value;

        if (!IsOwner && listing.Status != ListingStatus.Approved)
        {
            ErrorMessage = "Tin đăng hiện không khả dụng.";
            return Page();
        }

        Listing = listing;
        return Page();
    }

    public string FormatStatus(string status) => status switch
    {
        ListingStatus.Draft => "Bản nháp",
        ListingStatus.Pending => "Chờ duyệt",
        ListingStatus.Approved => "Đã duyệt",
        ListingStatus.Rejected => "Bị từ chối",
        ListingStatus.NeedsRevision => "Cần chỉnh sửa",
        ListingStatus.Hidden => "Đã ẩn",
        _ => status
    };
}



