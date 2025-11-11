using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class BatteryUpsertDto
{
    [Display(Name = "Loại pin")]
    [Required]
    [StringLength(50)]
    public string BatteryType { get; set; } = string.Empty;

    [Display(Name = "Thương hiệu")]
    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Display(Name = "Dung lượng")]
    [Required]
    [StringLength(50)]
    public string Capacity { get; set; } = string.Empty;

    [Display(Name = "Điện áp")]
    [StringLength(50)]
    public string? Voltage { get; set; }

    [Display(Name = "Tình trạng pin")]
    [Required]
    [StringLength(50)]
    public string Condition { get; set; } = string.Empty;

    [Display(Name = "Độ hao hụt / SOH (%)")]
    [Range(0, 100)]
    public int? HealthPercentage { get; set; }

    [Display(Name = "Giá bán mong muốn")]
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
    public decimal Price { get; set; }

    [Display(Name = "Mô tả chi tiết")]
    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Đường dẫn ảnh")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }
}

