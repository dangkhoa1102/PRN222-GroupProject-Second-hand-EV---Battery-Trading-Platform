using BLL.DTOs;

namespace BLL.Services;

public interface IVehicleListingService
{
    Task<List<VehicleListingDto>> GetMyListingsAsync(int sellerId, CancellationToken cancellationToken = default);
    Task<VehicleListingDetailDto?> GetListingDetailAsync(int vehicleId, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> CreateListingAsync(int sellerId, VehicleUpsertDto request, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> UpdateListingAsync(int sellerId, int vehicleId, VehicleUpsertDto request, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> SubmitForReviewAsync(int sellerId, int vehicleId, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> HideListingAsync(int sellerId, int vehicleId, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> DeleteListingAsync(int sellerId, int vehicleId, CancellationToken cancellationToken = default);
    Task<List<VehicleListingDto>> GetAllListingsAsync(CancellationToken cancellationToken = default);
    Task<List<VehicleListingDto>> GetApprovedListingsAsync(CancellationToken cancellationToken = default);
    Task<List<VehicleListingDto>> GetPendingListingsAsync(CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> ApproveAsync(int vehicleId, int staffId, string? note, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> RejectAsync(int vehicleId, int staffId, string note, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> RequestRevisionAsync(int vehicleId, int staffId, string note, CancellationToken cancellationToken = default);
    Task<ListingActionResultDto> DeleteListingAsAdminAsync(int vehicleId, int staffId, CancellationToken cancellationToken = default);
}

