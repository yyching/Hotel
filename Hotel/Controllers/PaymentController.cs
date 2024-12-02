using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class PaymentController : Controller
{
    private readonly DB db;

    public PaymentController(DB db)
    {
        this.db = db;
    }

    public IActionResult PaymentPage()
    {
        return View();
    }
}
