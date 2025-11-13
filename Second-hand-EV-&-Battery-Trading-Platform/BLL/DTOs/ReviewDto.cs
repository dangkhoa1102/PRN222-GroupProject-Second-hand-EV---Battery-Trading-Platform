namespace BLL.DTOs
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int OrderId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewedUserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
