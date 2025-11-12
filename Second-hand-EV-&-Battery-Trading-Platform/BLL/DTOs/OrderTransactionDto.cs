namespace BLL.DTOs
{
    public class OrderTransactionDto
    {
        public int OrderId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
