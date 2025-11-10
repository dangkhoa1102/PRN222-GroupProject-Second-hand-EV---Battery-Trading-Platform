using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroupProject.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        return SignOutUser();
    }

    public IActionResult OnPost()
    {
        return SignOutUser();
    }

    private IActionResult SignOutUser()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Account/Login");
    }
}

