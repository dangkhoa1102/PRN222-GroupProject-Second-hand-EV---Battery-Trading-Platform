using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace GroupProject.Pages.Admin
{
    public class BatteryTransactionsAdminModel : PageModel
    {
        private readonly IAdminTransactionService _service;

        public BatteryTransactionsAdminModel(IAdminTransactionService service)
        {
            _service = service;
        }

        public List<BatteryTransactionDto> Transactions { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            Transactions = await _service.GetBatteryTransactionsAsync();
            return Page();
        }
    }
}
