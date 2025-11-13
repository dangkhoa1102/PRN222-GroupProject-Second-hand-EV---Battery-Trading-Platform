using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.BatteryListings;

public class DetailsModel : PageModel
{
    private readonly IBatteryListingService _batteryListingService;
    private readonly IBuyerService _buyerService;

    public DetailsModel(IBatteryListingService batteryListingService, IBuyerService buyerService)
    {
        _batteryListingService = batteryListingService;
        _buyerService = buyerService;
    }

    public BatteryListingDetailDto? Listing { get; private set; }
    public List<Review> SellerReviews { get; private set; } = new();
    public string? ErrorMessage { get; private set; }
    public bool IsOwner { get; private set; }
    public bool IsBuyer { get; private set; }
    public bool CanPurchase { get; private set; }
    public string? ReturnUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, string? returnUrl = null)
    {
        // Trang công khai - không yêu cầu login
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        var listing = await _batteryListingService.GetListingDetailAsync(id);
        if (listing is null)
        {
            ErrorMessage = "Tin đăng không tồn tại.";
            return Page();
        }

        // Kiểm tra nếu user đã login
        if (userId.HasValue)
        {
            IsOwner = listing.SellerId == userId.Value;
            IsBuyer = role == RoleConstants.Customer && !IsOwner;
            
            // Buyer có thể mua nếu:
            // - Là customer (không phải seller của sản phẩm này)
            // - Sản phẩm đã được approved
            CanPurchase = IsBuyer && listing.Status == ListingStatus.Approved;
        }
        else
        {
            IsOwner = false;
            IsBuyer = false;
            CanPurchase = false;
        }

        // Chỉ cho phép xem nếu:
        // - Là owner (xem được mọi trạng thái)
        // - Hoặc sản phẩm đã được approved (public)
        if (!IsOwner && listing.Status != ListingStatus.Approved)
        {
            ErrorMessage = "Tin đăng hiện không khả dụng.";
            return Page();
        }

        // Load reviews của seller
        if (listing.SellerId > 0)
        {
            try
            {
                var reviews = await _buyerService.GetReview(listing.SellerId);
                SellerReviews = reviews.ToList();
            }
            catch
            {
                // Nếu không có reviews, để list rỗng
                SellerReviews = new List<Review>();
            }
        }

        // Lấy returnUrl từ query string, Referer header, hoặc sử dụng trang mặc định
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            // Kiểm tra query string
            returnUrl = Request.Query["returnUrl"].ToString();
        }
        
        // Nếu không có trong query string, lấy từ Referer header
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            {
                // Chỉ sử dụng referer nếu nó từ cùng domain và không phải là trang Details hiện tại
                var currentHost = Request.Host.Value;
                var currentPath = Request.Path.Value ?? "";
                if (refererUri.Host.Equals(currentHost, StringComparison.OrdinalIgnoreCase) 
                    && !refererUri.AbsolutePath.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
                {
                    returnUrl = refererUri.PathAndQuery;
                }
            }
        }
        
        // Nếu vẫn không có returnUrl, sử dụng trang mặc định dựa trên IsOwner
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            ReturnUrl = IsOwner ? "/BatteryListings/Index" : "/BatteryListings/Browse";
        }
        else
        {
            ReturnUrl = returnUrl;
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



