using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class VehicleUpsertDto
{
    [Display(Name = "Nhãn hiệu")]
    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Display(Name = "Dòng xe / Model")]
    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Display(Name = "Năm sản xuất")]
    [Range(1900, 2100, ErrorMessage = "Năm sản xuất không hợp lệ.")]
    public int? Year { get; set; }

    [Display(Name = "Dung lượng pin")]
    [StringLength(50)]
    public string? BatteryCapacity { get; set; }

    [Display(Name = "Giá bán mong muốn")]
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
    public decimal Price { get; set; }

    [Display(Name = "Tình trạng xe")]
    [Required]
    [StringLength(50)]
    public string Condition { get; set; } = string.Empty;

    [Display(Name = "Số km đã đi")]
    [Range(0, int.MaxValue)]
    public int? Mileage { get; set; }

    [Display(Name = "Mô tả chi tiết")]
    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Đường dẫn ảnh")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }
}

