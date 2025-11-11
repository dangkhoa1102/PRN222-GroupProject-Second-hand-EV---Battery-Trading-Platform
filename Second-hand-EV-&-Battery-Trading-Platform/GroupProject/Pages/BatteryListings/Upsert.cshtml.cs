using System.Globalization;
using System.IO;
using System.Linq;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.BatteryListings;

public class UpsertModel : PageModel
{
    private readonly IBatteryListingService _batteryListingService;
    private readonly IWebHostEnvironment _environment;
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    public UpsertModel(IBatteryListingService batteryListingService, IWebHostEnvironment environment)
    {
        _batteryListingService = batteryListingService;
        _environment = environment;
    }

    [BindProperty]
    public BatteryUpsertDto Input { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string PriceInput { get; set; } = string.Empty;

    [FromRoute]
    public int? Id { get; set; }

    public bool IsEdit => Id.HasValue;

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        if (Id.HasValue)
        {
            var existing = await _batteryListingService.GetListingDetailAsync(Id.Value);
            if (existing == null || existing.SellerId != sellerId.Value)
            {
                return NotFound();
            }

            Input = new BatteryUpsertDto
            {
                BatteryType = existing.BatteryType,
                Brand = existing.Brand,
                Capacity = existing.Capacity,
                Voltage = existing.Voltage,
                Condition = existing.Condition,
                HealthPercentage = existing.HealthPercentage,
                Price = existing.Price,
                Description = existing.Description,
                ImageUrl = existing.ImageUrl
            };
            PriceInput = existing.Price.ToString("0.##", CultureInfo.InvariantCulture);
        }
        else
        {
            PriceInput = string.Empty;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var sellerId = HttpContext.Session.GetInt32("UserId");
        if (!sellerId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        if (string.IsNullOrWhiteSpace(PriceInput))
        {
            ModelState.AddModelError("PriceInput", "Vui lòng nhập giá bán mong muốn.");
        }
        else if (!decimal.TryParse(PriceInput, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedPrice) &&
                 !decimal.TryParse(PriceInput, NumberStyles.Number, VietnameseCulture, out parsedPrice))
        {
            ModelState.AddModelError("PriceInput", "Giá trị không hợp lệ. Vui lòng nhập số, ví dụ 1200000.00.");
        }
        else
        {
            Input.Price = parsedPrice;
        }

        if (ImageFile != null && ImageFile.Length > 0)
        {
            if (!IsAllowedImage(ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Định dạng ảnh không hợp lệ (cho phép: jpg, jpeg, png, gif, webp).");
            }
            else if (ImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ImageFile", "Kích thước ảnh tối đa 5MB.");
            }
            else
            {
                var oldImage = Input.ImageUrl;
                Input.ImageUrl = await SaveImageAsync(ImageFile, "batteries");
                DeleteImage(oldImage);
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ListingActionResultDto result;
        if (IsEdit && Id.HasValue)
        {
            result = await _batteryListingService.UpdateListingAsync(sellerId.Value, Id.Value, Input);
        }
        else
        {
            result = await _batteryListingService.CreateListingAsync(sellerId.Value, Input);
        }

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        TempData["FlashMessage"] = IsEdit ? "Đã cập nhật tin pin." : "Đã tạo tin pin mới.";
        return RedirectToPage("Index");
    }

    private static bool IsAllowedImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedImageExtensions.Contains(ext);
    }

    private async Task<string> SaveImageAsync(IFormFile file, string folderName)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return $"/uploads/{folderName}/{fileName}";
    }

    private void DeleteImage(string? virtualPath)
    {
        if (string.IsNullOrWhiteSpace(virtualPath))
        {
            return;
        }

        var relativePath = virtualPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);
        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }
    }
}
