using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class HomeController : Controller
{
    private readonly DB db;

    public HomeController(DB db) 
    {  
        this.db = db; 
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult LoginRegister()
    {
        return View();
    }
}
