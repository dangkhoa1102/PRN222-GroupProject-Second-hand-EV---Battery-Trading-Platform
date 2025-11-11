using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.VehicleListings;

public class ReviewModel : PageModel
{
    private readonly IVehicleListingService _vehicleListingService;

    public ReviewModel(IVehicleListingService vehicleListingService)
    {
        _vehicleListingService = vehicleListingService;
    }

    [BindProperty]
    public string? ModerationNote { get; set; }

    public VehicleListingDetailDto? Listing { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanModerate { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!IsStaff())
        {
            return Forbid();
        }

        Listing = await _vehicleListingService.GetListingDetailAsync(id);

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
        return await HandleModerationAsync(id, staffId => _vehicleListingService.ApproveAsync(id, staffId, ModerationNote));
    }

    public async Task<IActionResult> OnPostNeedRevisionAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(ModerationNote))
        {
            ErrorMessage = "Vui lòng ghi rõ yêu cầu chỉnh sửa.";
            await LoadListing(id);
            return Page();
        }

        return await HandleModerationAsync(id, staffId => _vehicleListingService.RequestRevisionAsync(id, staffId, ModerationNote!));
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(ModerationNote))
        {
            ErrorMessage = "Vui lòng cung cấp lý do từ chối.";
            await LoadListing(id);
            return Page();
        }

        return await HandleModerationAsync(id, staffId => _vehicleListingService.RejectAsync(id, staffId, ModerationNote!));
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

        var result = await action(staffId.Value);
        TempData[result.IsSuccess ? "FlashMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Đã cập nhật trạng thái tin đăng."
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
        Listing = await _vehicleListingService.GetListingDetailAsync(id);
        CanModerate = Listing?.Status is ListingStatus.Pending or ListingStatus.NeedsRevision;
    }

    private bool IsStaff()
    {
        var role = HttpContext.Session.GetString("Role");
        return string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase);
    }
}

