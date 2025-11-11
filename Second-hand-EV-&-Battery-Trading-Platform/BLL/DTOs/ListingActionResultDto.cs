namespace BLL.DTOs;

public class ListingActionResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static ListingActionResultDto Success() => new() { IsSuccess = true };
    public static ListingActionResultDto Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}

