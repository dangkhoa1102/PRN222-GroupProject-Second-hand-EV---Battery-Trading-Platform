namespace BLL.DTOs
{
    public class VehicleTransactionDto
    {
        public int OrderId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
    }
}
