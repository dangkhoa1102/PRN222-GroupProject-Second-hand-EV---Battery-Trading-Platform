using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginRequestDto Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            return RedirectToPage("/Index");
        }

        if (TempData.TryGetValue("RegisterSuccess", out var message) && message is string success)
        {
            SuccessMessage = success;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.LoginAsync(Input);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        if (!result.UserId.HasValue)
        {
            ErrorMessage = "Không thể xác định người dùng.";
            return Page();
        }

        HttpContext.Session.SetInt32("UserId", result.UserId.Value);
        HttpContext.Session.SetString("FullName", result.FullName ?? string.Empty);
        HttpContext.Session.SetString("Role", result.Role ?? string.Empty);
        HttpContext.Session.SetString("Email", result.Email ?? string.Empty);

        return RedirectToPage("/Index");
    }
}

