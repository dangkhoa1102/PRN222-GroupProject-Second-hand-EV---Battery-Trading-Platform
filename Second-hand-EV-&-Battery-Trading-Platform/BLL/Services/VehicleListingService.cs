using BLL.Constants;
using BLL.DTOs;
using DAL.Models;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class VehicleListingService : IVehicleListingService
{
    public async Task<List<VehicleListingDto>> GetMyListingsAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicles = await vehicleRepository.GetBySellerAsync(sellerId, cancellationToken);

        return vehicles.Select(MapToListingDto).ToList();
    }

    public async Task<VehicleListingDetailDto?> GetListingDetailAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        return vehicle is null ? null : MapToDetailDto(vehicle);
    }

    public async Task<ListingActionResultDto> CreateListingAsync(int sellerId, VehicleUpsertDto request, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);

        var now = DateTime.UtcNow;
        var vehicle = new Vehicle
        {
            SellerId = sellerId,
            Brand = request.Brand,
            Model = request.Model,
            Year = request.Year,
            BatteryCapacity = request.BatteryCapacity,
            Price = request.Price,
            Condition = request.Condition,
            Mileage = request.Mileage,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Status = ListingStatus.Draft,
            IsActive = true,
            CreatedDate = now,
            UpdatedDate = now
        };

        await vehicleRepository.AddAsync(vehicle, cancellationToken);
        await vehicleRepository.SaveChangesAsync(cancellationToken);

        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> UpdateListingAsync(int sellerId, int vehicleId, VehicleUpsertDto request, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null || vehicle.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (vehicle.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền chỉnh sửa tin đăng này.");
        }

        if (vehicle.Status == ListingStatus.Rejected)
        {
            return ListingActionResultDto.Failure("Tin đăng đã bị từ chối, vui lòng tạo tin mới.");
        }

        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.BatteryCapacity = request.BatteryCapacity;
        vehicle.Price = request.Price;
        vehicle.Condition = request.Condition;
        vehicle.Mileage = request.Mileage;
        vehicle.Description = request.Description;
        vehicle.ImageUrl = request.ImageUrl;
        vehicle.UpdatedDate = DateTime.UtcNow;

        if (vehicle.Status == ListingStatus.NeedsRevision)
        {
            vehicle.Status = ListingStatus.Draft;
            vehicle.ModerationNote = null;
        }

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> SubmitForReviewAsync(int sellerId, int vehicleId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null || vehicle.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (vehicle.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền gửi duyệt tin đăng này.");
        }

        if (vehicle.Status is not ListingStatus.Draft and not ListingStatus.NeedsRevision)
        {
            return ListingActionResultDto.Failure("Chỉ có thể gửi duyệt tin ở trạng thái bản nháp hoặc cần chỉnh sửa.");
        }

        vehicle.Status = ListingStatus.Pending;
        vehicle.SubmittedDate = DateTime.UtcNow;
        vehicle.ModerationNote = null;

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> HideListingAsync(int sellerId, int vehicleId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null || vehicle.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (vehicle.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền ẩn tin đăng này.");
        }

        vehicle.Status = ListingStatus.Hidden;
        vehicle.IsActive = false;
        vehicle.UpdatedDate = DateTime.UtcNow;

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> DeleteListingAsync(int sellerId, int vehicleId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null || vehicle.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (vehicle.SellerId != sellerId)
        {
            return ListingActionResultDto.Failure("Bạn không có quyền xoá tin đăng này.");
        }

        // Kiểm tra xem có đơn hàng nào đang active không (Pending, Confirmed, Paid, Delivering, Delivered)
        var activeOrderStatuses = new[] { "Pending", "Confirmed", "Paid", "Delivering", "Delivered" };
        var hasActiveOrder = await context.VehicleOrders
            .Include(vo => vo.Order)
            .AnyAsync(vo => vo.VehicleId == vehicleId && 
                           activeOrderStatuses.Contains(vo.Order.OrderStatus), 
                      cancellationToken);

        if (hasActiveOrder)
        {
            return ListingActionResultDto.Failure("Không thể xóa tin đăng này vì có người đang chọn mua sản phẩm. Vui lòng đợi đơn hàng hoàn thành hoặc bị hủy.");
        }

        // Cho phép xóa bất kể status nào
        vehicle.IsActive = false;
        vehicle.Status = ListingStatus.Hidden;
        vehicle.UpdatedDate = DateTime.UtcNow;

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<List<VehicleListingDto>> GetAllListingsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicles = await vehicleRepository.GetAllAsync(cancellationToken);

        return vehicles.Select(MapToListingDto).ToList();
    }

    public async Task<List<VehicleListingDto>> GetApprovedListingsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicles = await vehicleRepository.GetByStatusesAsync(new[] { ListingStatus.Approved }, cancellationToken);

        return vehicles
            .OrderByDescending(v => v.PublishedDate ?? v.ApprovedDate ?? v.SubmittedDate ?? v.CreatedDate)
            .Select(MapToListingDto)
            .ToList();
    }

    public async Task<List<VehicleListingDto>> GetPendingListingsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicles = await vehicleRepository.GetByStatusesAsync(new[] { ListingStatus.Pending }, cancellationToken);

        return vehicles.Select(MapToListingDto).ToList();
    }

    public async Task<ListingActionResultDto> DeleteListingAsAdminAsync(int vehicleId, int staffId, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        vehicle.IsActive = false;
        vehicle.Status = ListingStatus.Hidden;
        vehicle.ApprovedBy = staffId;
        vehicle.ApprovedDate = DateTime.UtcNow;
        vehicle.ModerationNote = "Tin đăng đã bị quản trị viên gỡ bỏ.";
        vehicle.UpdatedDate = DateTime.UtcNow;

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> ApproveAsync(int vehicleId, int staffId, string? note, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null || vehicle.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (vehicle.Status != ListingStatus.Pending && vehicle.Status != ListingStatus.NeedsRevision)
        {
            return ListingActionResultDto.Failure("Chỉ có thể phê duyệt tin đăng đang chờ duyệt.");
        }

        vehicle.Status = ListingStatus.Approved;
        vehicle.ApprovedBy = staffId;
        vehicle.ApprovedDate = DateTime.UtcNow;
        vehicle.PublishedDate = DateTime.UtcNow;
        vehicle.ModerationNote = note;
        vehicle.IsActive = true;

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> RejectAsync(int vehicleId, int staffId, string note, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return ListingActionResultDto.Failure("Vui lòng cung cấp lý do từ chối.");
        }

        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null || vehicle.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (vehicle.Status != ListingStatus.Pending)
        {
            return ListingActionResultDto.Failure("Chỉ có thể từ chối tin đăng đang chờ duyệt.");
        }

        vehicle.Status = ListingStatus.Rejected;
        vehicle.ApprovedBy = staffId;
        vehicle.ApprovedDate = DateTime.UtcNow;
        vehicle.ModerationNote = note;

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    public async Task<ListingActionResultDto> RequestRevisionAsync(int vehicleId, int staffId, string note, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return ListingActionResultDto.Failure("Vui lòng cung cấp ghi chú chỉnh sửa.");
        }

        await using var context = new EVTradingPlatformContext();
        var vehicleRepository = new VehicleRepository(context);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null || vehicle.IsActive == false)
        {
            return ListingActionResultDto.Failure("Tin đăng không tồn tại.");
        }

        if (vehicle.Status != ListingStatus.Pending)
        {
            return ListingActionResultDto.Failure("Chỉ có thể yêu cầu chỉnh sửa tin đang chờ duyệt.");
        }

        vehicle.Status = ListingStatus.NeedsRevision;
        vehicle.ApprovedBy = staffId;
        vehicle.ApprovedDate = DateTime.UtcNow;
        vehicle.ModerationNote = note;

        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return ListingActionResultDto.Success();
    }

    private static VehicleListingDto MapToListingDto(Vehicle vehicle) =>
        new()
        {
            VehicleId = vehicle.VehicleId,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            Condition = vehicle.Condition,
            Status = vehicle.Status,
            SellerName = vehicle.Seller?.FullName,
            SellerEmail = vehicle.Seller?.Email,
            SellerPhoneNumber = vehicle.Seller?.PhoneNumber,
            BatteryCapacity = vehicle.BatteryCapacity,
            ImageUrl = vehicle.ImageUrl,
            SubmittedDate = vehicle.SubmittedDate,
            ApprovedDate = vehicle.ApprovedDate,
            PublishedDate = vehicle.PublishedDate,
            ModerationNote = vehicle.ModerationNote
        };

    private static VehicleListingDetailDto MapToDetailDto(Vehicle vehicle) =>
        new()
        {
            VehicleId = vehicle.VehicleId,
            SellerId = vehicle.SellerId,
            SellerName = vehicle.Seller?.FullName ?? string.Empty,
            SellerEmail = vehicle.Seller?.Email,
            SellerPhoneNumber = vehicle.Seller?.PhoneNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            BatteryCapacity = vehicle.BatteryCapacity,
            Price = vehicle.Price,
            Condition = vehicle.Condition,
            Mileage = vehicle.Mileage,
            Description = vehicle.Description,
            ImageUrl = vehicle.ImageUrl,
            Status = vehicle.Status,
            ModerationNote = vehicle.ModerationNote,
            SubmittedDate = vehicle.SubmittedDate,
            ApprovedDate = vehicle.ApprovedDate,
            PublishedDate = vehicle.PublishedDate
        };
}

