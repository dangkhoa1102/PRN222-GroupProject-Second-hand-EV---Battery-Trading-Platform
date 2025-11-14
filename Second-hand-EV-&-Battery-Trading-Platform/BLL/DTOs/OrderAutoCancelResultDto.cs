namespace BLL.DTOs;

public class OrderAutoCancelResultDto
{
    public int OrderId { get; set; }
    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime CancelledAt { get; set; }
}


