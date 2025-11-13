using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using GroupProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.BatteryListings;

public class ReviewModel : PageModel
{
    private readonly IBatteryListingService _batteryListingService;
    private readonly INotificationService _notificationService;

    public ReviewModel(IBatteryListingService batteryListingService, INotificationService notificationService)
    {
        _batteryListingService = batteryListingService;
        _notificationService = notificationService;
    }

    [BindProperty]
    public string? ModerationNote { get; set; }

    public BatteryListingDetailDto? Listing { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanModerate { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!IsStaff())
        {
            return Forbid();
        }

        Listing = await _batteryListingService.GetListingDetailAsync(id);

        if (Listing is null)
        {
            return NotFound();
        }

        if (Listing.Status == ListingStatus.Draft)
        {
            TempData["ErrorMessage"] = "Tin đang ở trạng thái bản nháp, không thể phê duyệt. Vui lòng yêu cầu người đăng gửi duyệt.";
            return RedirectToPage("Manage");
        }

        CanModerate = Listing.Status is ListingStatus.Pending or ListingStatus.NeedsRevision;

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        return await HandleModerationAsync(id, staffId => _batteryListingService.ApproveAsync(id, staffId, ModerationNote));
    }

    public async Task<IActionResult> OnPostNeedRevisionAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(ModerationNote))
        {
            ErrorMessage = "Vui lòng ghi rõ yêu cầu chỉnh sửa.";
            await LoadListing(id);
            return Page();
        }

        return await HandleModerationAsync(id, staffId => _batteryListingService.RequestRevisionAsync(id, staffId, ModerationNote!));
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(ModerationNote))
        {
            ErrorMessage = "Vui lòng cung cấp lý do từ chối.";
            await LoadListing(id);
            return Page();
        }

        return await HandleModerationAsync(id, staffId => _batteryListingService.RejectAsync(id, staffId, ModerationNote!));
    }

    private async Task<IActionResult> HandleModerationAsync(int id, Func<int, Task<ListingActionResultDto>> action)
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

        await LoadListing(id);

        if (Listing is null)
        {
            return RedirectToPage("Manage");
        }

        if (Listing.Status == ListingStatus.Draft)
        {
            TempData["ErrorMessage"] = "Tin đang ở trạng thái bản nháp, không thể phê duyệt. Vui lòng yêu cầu người đăng gửi duyệt.";
            return RedirectToPage("Manage");
        }

        if (Listing.Status is ListingStatus.Approved or ListingStatus.Rejected)
        {
            TempData["ErrorMessage"] = "Tin không ở trạng thái chờ duyệt hoặc cần chỉnh sửa, không thể cập nhật.";
            return RedirectToPage("Manage");
        }

        CanModerate = Listing.Status is ListingStatus.Pending or ListingStatus.NeedsRevision;

        var listingName = $"{Listing.Brand} {Listing.BatteryType}";
        var oldStatus = Listing.Status;
        var sellerId = Listing.SellerId;

        var result = await action(staffId.Value);
        
        if (result.IsSuccess)
        {
            // Reload listing để lấy status mới
            await LoadListing(id);
            
            if (Listing != null)
            {
                var newStatus = Listing.Status;
                
                // Gửi SignalR notification dựa trên status mới
                // So sánh bằng string để đảm bảo chính xác
                if (string.Equals(newStatus, ListingStatus.Approved, StringComparison.OrdinalIgnoreCase) && 
                    !string.Equals(oldStatus, ListingStatus.Approved, StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.NotifyListingApprovedAsync(id, "Battery", sellerId, listingName);
                }
                else if (string.Equals(newStatus, ListingStatus.Rejected, StringComparison.OrdinalIgnoreCase) && 
                         !string.Equals(oldStatus, ListingStatus.Rejected, StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.NotifyListingRejectedAsync(id, "Battery", sellerId, listingName, ModerationNote ?? "Không có lý do");
                }
                else if (string.Equals(newStatus, ListingStatus.NeedsRevision, StringComparison.OrdinalIgnoreCase) && 
                         !string.Equals(oldStatus, ListingStatus.NeedsRevision, StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.NotifyListingNeedsRevisionAsync(id, "Battery", sellerId, listingName, ModerationNote ?? "Cần chỉnh sửa");
                }
            }
        }

        TempData[result.IsSuccess ? "FlashMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Đã cập nhật trạng thái tin pin."
            : result.ErrorMessage;

        if (!result.IsSuccess)
        {
            if (Listing is null)
            {
                return RedirectToPage("Manage");
            }
            return Page();
        }

        return RedirectToPage("Manage");
    }

    private async Task LoadListing(int id)
    {
        Listing = await _batteryListingService.GetListingDetailAsync(id);
        CanModerate = Listing?.Status is ListingStatus.Pending or ListingStatus.NeedsRevision;
    }

    private bool IsStaff()
    {
        var role = HttpContext.Session.GetString("Role");
        return string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase);
    }
}

