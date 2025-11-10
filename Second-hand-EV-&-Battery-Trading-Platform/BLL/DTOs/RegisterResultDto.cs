namespace BLL.DTOs;

public class RegisterResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? UserId { get; set; }
    public string? Email { get; set; }
}

