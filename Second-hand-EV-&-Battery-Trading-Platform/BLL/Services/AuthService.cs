using BLL.Constants;
using BLL.DTOs;
using DAL.Models;
using DAL.Repository;

namespace BLL.Services;

public class AuthService : IAuthService
{
    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var userRepository = new UserRepository(context);
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return new LoginResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Email hoặc mật khẩu không đúng."
            };
        }

        if (user.IsActive.HasValue && !user.IsActive.Value)
        {
            return new LoginResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Tài khoản của bạn đã bị vô hiệu hóa."
            };
        }

        if (!VerifyPassword(user, request.Password))
        {
            return new LoginResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Email hoặc mật khẩu không đúng."
            };
        }

        if (!RoleConstants.TryNormalize(user.Role, out var normalizedRole))
        {
            return new LoginResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Vai trò tài khoản không hợp lệ."
            };
        }

        return new LoginResultDto
        {
            IsSuccess = true,
            UserId = user.UserId,
            FullName = user.FullName,
            Role = normalizedRole,
            Email = user.Email
        };
    }

    public async Task<RegisterResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var context = new EVTradingPlatformContext();
        var userRepository = new UserRepository(context);

        if (!RoleConstants.TryNormalize(RoleConstants.Customer, out var normalizedRole))
        {
            return new RegisterResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Vai trò mặc định không hợp lệ."
            };
        }

        if (await userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return new RegisterResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Email đã tồn tại."
            };
        }

        var user = new User
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Role = normalizedRole,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new RegisterResultDto
        {
            IsSuccess = true,
            UserId = user.UserId,
            Email = user.Email
        };
    }

    private static bool VerifyPassword(User user, string providedPassword) =>
        !string.IsNullOrEmpty(user.Password) &&
        string.Equals(user.Password, providedPassword, StringComparison.Ordinal);
}

