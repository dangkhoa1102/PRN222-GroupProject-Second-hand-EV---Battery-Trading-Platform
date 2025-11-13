namespace BLL.DTOs
{
    public class BatteryTransactionDto
    {
        public int OrderId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string BatteryBrand { get; set; } = string.Empty;
        public string BatteryType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
    }
}
