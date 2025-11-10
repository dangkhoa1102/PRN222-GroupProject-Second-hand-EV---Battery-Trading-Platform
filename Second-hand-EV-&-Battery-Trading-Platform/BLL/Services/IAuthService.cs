using BLL.DTOs;

namespace BLL.Services;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<RegisterResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
}

