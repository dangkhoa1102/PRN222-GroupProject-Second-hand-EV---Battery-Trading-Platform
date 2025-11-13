using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;



    public class CreateReviewDto
    {
    [Required(ErrorMessage = "Mã đơn hàng là bắt buộc.")]
        public int OrderId { get; set; }

        public int ReviewerId { get; set; }

        public int ReviewedUserId { get; set; }

    [Required(ErrorMessage = "Đánh giá là bắt buộc.")]
    [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Nhận xét không được vượt quá 1000 ký tự.")]
        public string? Comment { get; set; }


        public DateTime? CreatedDate { get; set; }
    }

