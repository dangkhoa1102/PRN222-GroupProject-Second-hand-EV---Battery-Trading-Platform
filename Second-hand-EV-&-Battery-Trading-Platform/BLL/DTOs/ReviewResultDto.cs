namespace BLL.DTOs;

public class ReviewResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ReviewId { get; set; }

    public static ReviewResultDto Success(int reviewId) => new() 
    { 
        IsSuccess = true, 
        ReviewId = reviewId 
    };

    public static ReviewResultDto Failure(string message) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = message 
    };
}

