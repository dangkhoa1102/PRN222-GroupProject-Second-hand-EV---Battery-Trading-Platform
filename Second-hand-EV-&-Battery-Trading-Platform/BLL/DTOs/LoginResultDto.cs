namespace BLL.DTOs;

public class LoginResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? Email { get; set; }
}

