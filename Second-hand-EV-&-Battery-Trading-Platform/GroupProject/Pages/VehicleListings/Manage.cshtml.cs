using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using GroupProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.VehicleListings;

public class ManageModel : PageModel
{
    private readonly IVehicleListingService _vehicleListingService;
    private readonly INotificationService _notificationService;

    public ManageModel(IVehicleListingService vehicleListingService, INotificationService notificationService)
    {
        _vehicleListingService = vehicleListingService;
        _notificationService = notificationService;
    }

    public List<VehicleListingDto> Listings { get; private set; } = new();
    public string? FlashMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsStaff())
        {
            return Forbid();
        }

        Listings = await _vehicleListingService.GetAllListingsAsync();

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

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!IsStaff())
        {
            return Forbid();
        }

        var staffId = HttpContext.Session.GetInt32("UserId");
        if (!staffId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var listing = await _vehicleListingService.GetListingDetailAsync(id);
        var listingName = listing != null ? $"{listing.Brand} {listing.Model}" : $"Tin đăng #{id}";
        var sellerId = listing?.SellerId ?? 0;

        var result = await _vehicleListingService.DeleteListingAsAdminAsync(id, staffId.Value);
        
        if (result.IsSuccess && sellerId > 0)
        {
            await _notificationService.NotifyListingDeletedAsync(id, "Vehicle", sellerId, listingName);
            await _notificationService.NotifyAdminAsync($"Đã xóa tin đăng: {listingName}", "info");
        }

        TempData[result.IsSuccess ? "FlashMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Đã gỡ tin xe khỏi hệ thống."
            : result.ErrorMessage;

        return RedirectToPage();
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

    private bool IsStaff()
    {
        var role = HttpContext.Session.GetString("Role");
        return string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase);
    }
}

