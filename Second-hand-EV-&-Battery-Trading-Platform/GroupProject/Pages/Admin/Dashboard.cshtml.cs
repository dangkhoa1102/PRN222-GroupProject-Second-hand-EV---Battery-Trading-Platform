using System.Linq;
using System.Text.Json;
using BLL.Constants;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IAdminTransactionService _transactionService;

    public DashboardModel(IAdminTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public AdminDashboardStatsDto Stats { get; private set; } = new();
    public string TrendLabelsJson { get; private set; } = "[]";
    public string TrendOrdersJson { get; private set; } = "[]";
    public string TrendRevenueJson { get; private set; } = "[]";
    public string StatusBreakdownLabelsJson { get; private set; } = "[]";
    public string StatusBreakdownCountsJson { get; private set; } = "[]";

    [BindProperty(SupportsGet = true)]
    public int Months { get; set; } = 6;

    public async Task<IActionResult> OnGetAsync()
    {
        var role = HttpContext.Session.GetString("Role");
        if (string.IsNullOrEmpty(role) || !role.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Account/Login");
        }

        Stats = await _transactionService.GetDashboardStatsAsync(Months);
        TrendLabelsJson = JsonSerializer.Serialize(Stats.MonthlyTrends.Select(t => t.Label));
        TrendOrdersJson = JsonSerializer.Serialize(Stats.MonthlyTrends.Select(t => t.OrderCount));
        TrendRevenueJson = JsonSerializer.Serialize(Stats.MonthlyTrends.Select(t => t.Revenue));
        StatusBreakdownLabelsJson = JsonSerializer.Serialize(Stats.StatusBreakdown.Select(s => s.Status));
        StatusBreakdownCountsJson = JsonSerializer.Serialize(Stats.StatusBreakdown.Select(s => s.Count));

        return Page();
    }
}

