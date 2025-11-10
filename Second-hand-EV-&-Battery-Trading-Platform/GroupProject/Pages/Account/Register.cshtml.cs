using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public RegisterRequestDto Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.RegisterAsync(Input);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        TempData["RegisterSuccess"] = "Đăng ký thành công, vui lòng đăng nhập.";
        return RedirectToPage("/Account/Login");
    }
}

