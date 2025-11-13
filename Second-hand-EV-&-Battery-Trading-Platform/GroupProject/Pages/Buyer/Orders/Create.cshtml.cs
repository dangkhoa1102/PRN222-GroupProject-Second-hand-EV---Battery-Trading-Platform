using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GroupProject.Pages.Buyer.Orders;

public class CreateModel : PageModel
{
    private readonly IBuyerService _buyerService;
    private readonly IVehicleListingService _vehicleListingService;
    private readonly IBatteryListingService _batteryListingService;

    public CreateModel(
        IBuyerService buyerService,
        IVehicleListingService vehicleListingService,
        IBatteryListingService batteryListingService)
    {
        _buyerService = buyerService;
        _vehicleListingService = vehicleListingService;
        _batteryListingService = batteryListingService;
    }

    [BindProperty]
    public CreateOrderDto OrderDto { get; set; } = new();

    public string ItemType { get; private set; } = string.Empty;
    public int ItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public decimal ItemPrice { get; private set; }
    public int SellerId { get; private set; }
    public string SellerName { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public List<SelectListItem> PaymentMethods { get; private set; } = new()
    {
        new SelectListItem { Value = "Cash", Text = "Tiền mặt" },
        new SelectListItem { Value = "BankTransfer", Text = "Chuyển khoản ngân hàng" },
        new SelectListItem { Value = "CreditCard", Text = "Thẻ tín dụng" },
        new SelectListItem { Value = "E-Wallet", Text = "Ví điện tử" }
    };

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        // Kiểm tra login
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        // Kiểm tra role
        if (role != RoleConstants.Customer)
        {
            ErrorMessage = "Chỉ khách hàng mới có thể tạo đơn hàng.";
            return Page();
        }

        // Lấy parameters từ query string
        var itemType = Request.Query["itemType"].ToString();
        var itemIdStr = Request.Query["itemId"].ToString();
        
        if (string.IsNullOrEmpty(itemType) || string.IsNullOrEmpty(itemIdStr) || !int.TryParse(itemIdStr, out var itemId) || itemId <= 0)
        {
            ErrorMessage = "Thông tin sản phẩm không hợp lệ. Vui lòng chọn sản phẩm từ trang chi tiết.";
            return Page();
        }

        ItemType = itemType;
        ItemId = itemId;

        // Lấy thông tin sản phẩm
        if (itemType == "Vehicle")
        {
            var vehicle = await _vehicleListingService.GetListingDetailAsync(itemId);
            if (vehicle == null)
            {
                ErrorMessage = "Không tìm thấy sản phẩm.";
                return Page();
            }

            if (vehicle.Status != ListingStatus.Approved)
            {
                ErrorMessage = "Sản phẩm chưa được duyệt hoặc không khả dụng.";
                return Page();
            }

            if (vehicle.SellerId == userId.Value)
            {
                ErrorMessage = "Bạn không thể mua sản phẩm của chính mình.";
                return Page();
            }

            ItemName = $"{vehicle.Brand} {vehicle.Model}";
            ItemPrice = vehicle.Price;
            SellerId = vehicle.SellerId;
            SellerName = vehicle.SellerName;
        }
        else if (itemType == "Battery")
        {
            var battery = await _batteryListingService.GetListingDetailAsync(itemId);
            if (battery == null)
            {
                ErrorMessage = "Không tìm thấy sản phẩm.";
                return Page();
            }

            if (battery.Status != ListingStatus.Approved)
            {
                ErrorMessage = "Sản phẩm chưa được duyệt hoặc không khả dụng.";
                return Page();
            }

            if (battery.SellerId == userId.Value)
            {
                ErrorMessage = "Bạn không thể mua sản phẩm của chính mình.";
                return Page();
            }

            ItemName = $"{battery.Brand} {battery.BatteryType}";
            ItemPrice = battery.Price;
            SellerId = battery.SellerId;
            SellerName = battery.SellerName;
        }
        else
        {
            ErrorMessage = "Loại sản phẩm không hợp lệ.";
            return Page();
        }

        // Set giá trị mặc định
        OrderDto.BuyerId = userId.Value;
        OrderDto.SellerId = SellerId;
        OrderDto.TotalAmount = ItemPrice;
        OrderDto.ItemType = ItemType;
        OrderDto.ItemId = ItemId;
        OrderDto.PaymentMethod = "Cash"; // Mặc định

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        // Kiểm tra login
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        // Kiểm tra role
        if (role != RoleConstants.Customer)
        {
            ErrorMessage = "Chỉ khách hàng mới có thể tạo đơn hàng.";
            await LoadItemInfoAsync(); // Load lại thông tin để hiển thị
            return Page();
        }

        // Validate OrderDto có đầy đủ thông tin
        if (OrderDto.BuyerId <= 0 || OrderDto.SellerId <= 0 || OrderDto.ItemId <= 0 || string.IsNullOrEmpty(OrderDto.ItemType))
        {
            ErrorMessage = "Thông tin đơn hàng không hợp lệ. Vui lòng thử lại từ đầu.";
            // Reload thông tin sản phẩm từ ItemType và ItemId
            await LoadItemInfoAsync();
            return Page();
        }

        // Validate PaymentMethod
        if (string.IsNullOrWhiteSpace(OrderDto.PaymentMethod))
        {
            ModelState.AddModelError(nameof(OrderDto.PaymentMethod), "Vui lòng chọn phương thức thanh toán.");
        }

        if (!ModelState.IsValid)
        {
            // Reload thông tin sản phẩm để hiển thị lại form
            await LoadItemInfoAsync();
            return Page();
        }

        try
        {
            // Tạo đơn hàng
            var result = await _buyerService.CreateOrder(OrderDto);
            if (result)
            {
                return RedirectToPage("/Buyer/Orders/Index", new { message = "Đơn hàng đã được tạo thành công. Vui lòng chờ người bán xác nhận." });
            }
            else
            {
                ErrorMessage = "Không thể tạo đơn hàng. Vui lòng thử lại.";
                await LoadItemInfoAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
            await LoadItemInfoAsync();
            return Page();
        }
    }

    private async Task LoadItemInfoAsync()
    {
        // Sử dụng ItemType và ItemId từ model properties hoặc OrderDto
        var itemType = !string.IsNullOrEmpty(ItemType) ? ItemType : OrderDto.ItemType;
        var itemId = ItemId > 0 ? ItemId : OrderDto.ItemId;

        if (string.IsNullOrEmpty(itemType) || itemId <= 0)
        {
            return; // Không có thông tin để load
        }

        if (itemType == "Vehicle")
        {
            var vehicle = await _vehicleListingService.GetListingDetailAsync(itemId);
            if (vehicle != null)
            {
                ItemName = $"{vehicle.Brand} {vehicle.Model}";
                ItemPrice = vehicle.Price;
                SellerId = vehicle.SellerId;
                SellerName = vehicle.SellerName;
                ItemType = itemType;
                ItemId = itemId;
            }
        }
        else if (itemType == "Battery")
        {
            var battery = await _batteryListingService.GetListingDetailAsync(itemId);
            if (battery != null)
            {
                ItemName = $"{battery.Brand} {battery.BatteryType}";
                ItemPrice = battery.Price;
                SellerId = battery.SellerId;
                SellerName = battery.SellerName;
                ItemType = itemType;
                ItemId = itemId;
            }
        }
    }
}

