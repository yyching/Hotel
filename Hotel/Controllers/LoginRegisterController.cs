using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class LoginRegisterController : Controller
{
    private readonly DB db;

    public LoginRegisterController(DB db)
    {
        this.db = db;
    }

    public IActionResult LoginRegister()
    {
        return View();
    }
}