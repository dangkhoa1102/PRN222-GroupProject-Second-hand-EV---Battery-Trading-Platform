using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.VehicleListings;

public class PendingModel : PageModel
{
    private readonly IVehicleListingService _vehicleListingService;

    public PendingModel(IVehicleListingService vehicleListingService)
    {
        _vehicleListingService = vehicleListingService;
    }

    public List<VehicleListingDetailDto> Listings { get; private set; } = new();
    public string? FlashMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsStaff())
        {
            return Forbid();
        }

        Listings = (await _vehicleListingService.GetPendingListingsAsync())
            .Select(detail => new VehicleListingDetailDto
            {
                VehicleId = detail.VehicleId,
                Brand = detail.Brand,
                Model = detail.Model,
                Price = detail.Price,
                SellerName = detail.SellerName ?? "Không rõ",
                SellerEmail = detail.SellerEmail,
                SellerPhoneNumber = detail.SellerPhoneNumber,
                SubmittedDate = detail.SubmittedDate
            }).ToList();

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

    private bool IsStaff()
    {
        var role = HttpContext.Session.GetString("Role");
        return string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase);
    }
}

