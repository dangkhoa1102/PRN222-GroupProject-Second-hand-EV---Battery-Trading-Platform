using BLL.Constants;
using BLL.DTOs;
using DAL.Models;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BatteryListingService : IBatteryListingService
{
    public async Task<List<BatteryListingDto>> GetMyListingsAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var batteries = await repository.GetBySellerAsync(sellerId, cancellationToken);
        return batteries.Select(MapToListingDto).ToList();
    }

    public async Task<BatteryListingDetailDto?> GetListingDetailAsync(int batteryId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);
        return battery is null ? null : MapToDetailDto(battery);
    }

    public async Task<ListingActionResultDto> CreateListingAsync(int sellerId, BatteryUpsertDto request, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);

        var now = DateTime.UtcNow;
        var battery = new Battery
        {
            SellerId = sellerId,
            BatteryType = request.BatteryType,
            Brand = request.Brand,
            Capacity = request.Capacity,
            Voltage = request.Voltage,
            Condition = request.Condition,
            HealthPercentage = request.HealthPercentage,
            Price = request.Price,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Status = ListingStatus.Draft,
            IsActive = true,
            CreatedDate = now,
            UpdatedDate = now
        };

        await repository.AddAsync(battery, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> UpdateListingAsync(int sellerId, int batteryId, BatteryUpsertDto request, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null || battery.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (battery.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền chỉnh sửa tin đăng này.");
        }

        if (battery.Status == ListingStatus.Rejected)
        {
            return ListingActionResultDto.Failure("Tin đăng đã bị từ chối, vui lòng tạo tin mới.");
        }

        battery.BatteryType = request.BatteryType;
        battery.Brand = request.Brand;
        battery.Capacity = request.Capacity;
        battery.Voltage = request.Voltage;
        battery.Condition = request.Condition;
        battery.HealthPercentage = request.HealthPercentage;
        battery.Price = request.Price;
        battery.Description = request.Description;
        battery.ImageUrl = request.ImageUrl;
        battery.UpdatedDate = DateTime.UtcNow;

        if (battery.Status is ListingStatus.NeedsRevision)
        {
            battery.Status = ListingStatus.Draft;
            battery.ModerationNote = null;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> SubmitForReviewAsync(int sellerId, int batteryId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null || battery.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (battery.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền gửi duyệt tin đăng này.");
        }

        if (battery.Status is not ListingStatus.Draft and not ListingStatus.NeedsRevision)
        {
            return ListingActionResultDto.Failure("Chỉ có thể gửi duyệt tin ở trạng thái bản nháp hoặc cần chỉnh sửa.");
        }

        battery.Status = ListingStatus.Pending;
        battery.SubmittedDate = DateTime.UtcNow;
        battery.ModerationNote = null;

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> HideListingAsync(int sellerId, int batteryId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null || battery.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (battery.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền ẩn tin đăng này.");
        }

        battery.Status = ListingStatus.Hidden;
        battery.IsActive = false;
        battery.UpdatedDate = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> DeleteListingAsync(int sellerId, int batteryId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null || battery.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (battery.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền xoá tin đăng này.");
        }

        // Kiểm tra xem có đơn hàng nào đang active không (Pending, Confirmed, Paid, Delivering, Delivered)
        var activeOrderStatuses = new[] { "Pending", "Confirmed", "Paid", "Delivering", "Delivered" };
        var hasActiveOrder = await context.BatteryOrders
            .Include(bo => bo.Order)
            .AnyAsync(bo => bo.BatteryId == batteryId && 
                           activeOrderStatuses.Contains(bo.Order.OrderStatus), 
                      cancellationToken);

        if (hasActiveOrder)
        {
            return ListingActionResultDto.Failure("Không thể xóa tin đăng này vì có người đang chọn mua sản phẩm. Vui lòng đợi đơn hàng hoàn thành hoặc bị hủy.");
        }

        // Cho phép xóa bất kể status nào
        battery.IsActive = false;
        battery.Status = ListingStatus.Hidden;
        battery.UpdatedDate = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<List<BatteryListingDto>> GetAllListingsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var batteries = await repository.GetAllAsync(cancellationToken);
        return batteries.Select(MapToListingDto).ToList();
    }

    public async Task<List<BatteryListingDto>> GetApprovedListingsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var batteries = await repository.GetByStatusesAsync(new[] { ListingStatus.Approved }, cancellationToken);
        return batteries
            .OrderByDescending(b => b.PublishedDate ?? b.ApprovedDate ?? b.SubmittedDate ?? b.CreatedDate)
            .Select(MapToListingDto)
            .ToList();
    }

    public async Task<List<BatteryListingDto>> GetPendingListingsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var batteries = await repository.GetByStatusesAsync(new[] { ListingStatus.Pending }, cancellationToken);
        return batteries.Select(MapToListingDto).ToList();
    }

    public async Task<ListingActionResultDto> DeleteListingAsAdminAsync(int batteryId, int staffId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        battery.IsActive = false;
        battery.Status = ListingStatus.Hidden;
        battery.ApprovedBy = staffId;
        battery.ApprovedDate = DateTime.UtcNow;
        battery.ModerationNote = "Tin đăng đã bị quản trị viên gỡ bỏ.";
        battery.UpdatedDate = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> ApproveAsync(int batteryId, int staffId, string? note, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null || battery.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (battery.Status != ListingStatus.Pending && battery.Status != ListingStatus.NeedsRevision)
        {
            return ListingActionResultDto.Failure("Chỉ có thể phê duyệt tin đăng đang chờ duyệt.");
        }

        battery.Status = ListingStatus.Approved;
        battery.ApprovedBy = staffId;
        battery.ApprovedDate = DateTime.UtcNow;
        battery.PublishedDate = DateTime.UtcNow;
        battery.ModerationNote = note;
        battery.IsActive = true;

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> RejectAsync(int batteryId, int staffId, string note, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return ListingActionResultDto.Failure("Vui lòng cung cấp lý do từ chối.");
        }

        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null || battery.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (battery.Status != ListingStatus.Pending)
        {
            return ListingActionResultDto.Failure("Chỉ có thể từ chối tin đăng đang chờ duyệt.");
        }

        battery.Status = ListingStatus.Rejected;
        battery.ApprovedBy = staffId;
        battery.ApprovedDate = DateTime.UtcNow;
        battery.ModerationNote = note;

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> RequestRevisionAsync(int batteryId, int staffId, string note, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return ListingActionResultDto.Failure("Vui lòng cung cấp ghi chú chỉnh sửa.");
        }

        await using var context = new EVTradingPlatformContext();
        var repository = new BatteryRepository(context);
        var battery = await repository.GetByIdAsync(batteryId, cancellationToken);

        if (battery is null || battery.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (battery.Status != ListingStatus.Pending)
        {
            return ListingActionResultDto.Failure("Chỉ có thể yêu cầu chỉnh sửa tin đang chờ duyệt.");
        }

        battery.Status = ListingStatus.NeedsRevision;
        battery.ApprovedBy = staffId;
        battery.ApprovedDate = DateTime.UtcNow;
        battery.ModerationNote = note;

        await repository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    private static BatteryListingDto MapToListingDto(Battery battery) =>
        new()
        {
            BatteryId = battery.BatteryId,
            BatteryType = battery.BatteryType,
            Brand = battery.Brand,
            Capacity = battery.Capacity,
            Voltage = battery.Voltage,
            Condition = battery.Condition,
            HealthPercentage = battery.HealthPercentage,
            Price = battery.Price,
            Status = battery.Status,
            SellerName = battery.Seller?.FullName,
            SellerEmail = battery.Seller?.Email,
            SellerPhoneNumber = battery.Seller?.PhoneNumber,
            ImageUrl = battery.ImageUrl,
            SubmittedDate = battery.SubmittedDate,
            ApprovedDate = battery.ApprovedDate,
            PublishedDate = battery.PublishedDate,
            ModerationNote = battery.ModerationNote
        };

    private static BatteryListingDetailDto MapToDetailDto(Battery battery) =>
        new()
        {
            BatteryId = battery.BatteryId,
            SellerId = battery.SellerId,
            SellerName = battery.Seller?.FullName ?? string.Empty,
            SellerEmail = battery.Seller?.Email,
            SellerPhoneNumber = battery.Seller?.PhoneNumber,
            BatteryType = battery.BatteryType,
            Brand = battery.Brand,
            Capacity = battery.Capacity,
            Voltage = battery.Voltage,
            Condition = battery.Condition,
            HealthPercentage = battery.HealthPercentage,
            Price = battery.Price,
            Description = battery.Description,
            ImageUrl = battery.ImageUrl,
            Status = battery.Status,
            ModerationNote = battery.ModerationNote,
            SubmittedDate = battery.SubmittedDate,
            ApprovedDate = battery.ApprovedDate,
            PublishedDate = battery.PublishedDate
        };
}

