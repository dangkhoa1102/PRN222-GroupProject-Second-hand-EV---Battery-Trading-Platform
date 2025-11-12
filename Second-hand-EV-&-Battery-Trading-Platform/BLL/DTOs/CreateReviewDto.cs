using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class CreateReviewDto
    {
        public int OrderId { get; set; }

        public int ReviewerId { get; set; }

        public int ReviewedUserId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
