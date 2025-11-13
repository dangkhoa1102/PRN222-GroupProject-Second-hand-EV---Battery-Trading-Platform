using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace GroupProject.Pages.Admin
{
    public class OrderTransactionsAdminModel : PageModel
    {
        private readonly IAdminTransactionService _service;

        public OrderTransactionsAdminModel(IAdminTransactionService service)
        {
            _service = service;
        }

        public List<OrderTransactionDto> Transactions { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            Transactions = await _service.GetAllOrdersAsync();
            return Page();
        }
    }
}
