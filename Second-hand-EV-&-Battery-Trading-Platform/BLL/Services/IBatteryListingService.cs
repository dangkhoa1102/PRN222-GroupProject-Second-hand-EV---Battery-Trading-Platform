using BLL.DTOs;

namespace BLL.Services;

public interface IBatteryListingService
{
    Task<List<BatteryListingDto>> GetMyListingsAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<BatteryListingDetailDto?> GetListingDetailAsync(int batteryId, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> CreateListingAsync(int sellerId, BatteryUpsertDto request, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> UpdateListingAsync(int sellerId, int batteryId, BatteryUpsertDto request, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> SubmitForReviewAsync(int sellerId, int batteryId, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> HideListingAsync(int sellerId, int batteryId, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> DeleteListingAsync(int sellerId, int batteryId, CancellationToken cancellationToken = default);
    Task<List<BatteryListingDto>> GetAllListingsAsync(CancellationToken cancellationToken = default);
    Task<List<BatteryListingDto>> GetApprovedListingsAsync(CancellationToken cancellationToken = default);
    Task<List<BatteryListingDto>> GetPendingListingsAsync(CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> ApproveAsync(int batteryId, int staffId, string? note, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> RejectAsync(int batteryId, int staffId, string note, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> RequestRevisionAsync(int batteryId, int staffId, string note, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> DeleteListingAsAdminAsync(int batteryId, int staffId, CancellationToken cancellationToken = default);
}

