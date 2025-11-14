using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using GroupProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.VehicleListings;

public class IndexModel : PageModel
{
    private readonly IVehicleListingService _vehicleListingService;
    private readonly INotificationService _notificationService;

    public IndexModel(IVehicleListingService vehicleListingService, INotificationService notificationService)
    {
        _vehicleListingService = vehicleListingService;
        _notificationService = notificationService;
    }

    public List<VehicleListingDto> Listings { get; private set; } = new();
    public string? FlashMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool CanSubmit(string status) =>
        status is ListingStatus.Draft or ListingStatus.NeedsRevision;

    public bool CanEdit(string status) =>
        status is ListingStatus.Draft or ListingStatus.NeedsRevision or ListingStatus.Approved;

    public bool CanDelete(string status) =>
        status is ListingStatus.Draft or ListingStatus.Hidden;

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

    public string GetStatusCssClass(string status) => status switch
    {
        ListingStatus.Draft => "status-badge--draft",
        ListingStatus.Pending => "status-badge--pending",
        ListingStatus.Approved => "status-badge--approved",
        ListingStatus.Rejected => "status-badge--rejected",
        ListingStatus.NeedsRevision => "status-badge--revision",
        ListingStatus.Hidden => "status-badge--hidden",
        _ => string.Empty
    };

    public async Task<IActionResult> OnGetAsync()
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        Listings = await _vehicleListingService.GetMyListingsAsync(sellerId.Value);

        if (TempData.TryGetValue("FlashMessage", out var message) && message is string success && !string.IsNullOrWhiteSpace(success))
        {
            FlashMessage = success;
        }

        if (TempData.TryGetValue("ErrorMessage", out var err) && err is string error && !string.IsNullOrWhiteSpace(error))
        {
            ErrorMessage = error;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int id)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var listing = await _vehicleListingService.GetListingDetailAsync(id);
        var listingName = listing != null ? $"{listing.Brand} {listing.Model}" : $"Tin đăng #{id}";

        var result = await _vehicleListingService.SubmitForReviewAsync(sellerId.Value, id);
        
        if (result.IsSuccess)
        {
            await _notificationService.NotifyListingSubmittedAsync(id, "Vehicle", sellerId.Value, listingName);
        }

        TempData[result.IsSuccess ? "FlashMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Đã gửi tin đăng lên staff phê duyệt."
            : result.ErrorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostHideAsync(int id)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var listing = await _vehicleListingService.GetListingDetailAsync(id);
        var listingName = listing != null ? $"{listing.Brand} {listing.Model}" : $"Tin đăng #{id}";

        var result = await _vehicleListingService.HideListingAsync(sellerId.Value, id);
        
        if (result.IsSuccess)
        {
            await _notificationService.NotifyListingHiddenAsync(id, "Vehicle", sellerId.Value, listingName);
        }

        TempData[result.IsSuccess ? "FlashMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Đã tạm ẩn tin đăng."
            : result.ErrorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var listing = await _vehicleListingService.GetListingDetailAsync(id);
        var listingName = listing != null ? $"{listing.Brand} {listing.Model}" : $"Tin đăng #{id}";

        var result = await _vehicleListingService.DeleteListingAsync(sellerId.Value, id);
        
        if (result.IsSuccess)
        {
            await _notificationService.NotifyListingDeletedAsync(id, "Vehicle", sellerId.Value, listingName);
        }

        TempData[result.IsSuccess ? "FlashMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Đã xóa tin đăng."
            : result.ErrorMessage;

        return RedirectToPage();
    }
}

