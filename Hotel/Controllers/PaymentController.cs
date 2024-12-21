using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class PaymentController : Controller
{
    private readonly DB db;

    public PaymentController(DB db)
    {
        this.db = db;
    }

    public IActionResult PaymentPage(string? categoryID, DateOnly checkIn, DateOnly checkOut, string[]? foodServiceIds, int[]? foodQuantities, string[]? roomServiceIds, int[]? roomQuantities)
    {
        ViewBag.categoryID = categoryID;
        ViewBag.checkIn = checkIn;
        ViewBag.checkOut = checkOut;

        var selectedFoodServices = foodServiceIds
       .Zip(foodQuantities, (id, qty) => new { ServiceId = id, Quantity = qty })
       .Where(x => x.Quantity > 0)
       .ToList();
        ViewBag.SelectedFoodServices = selectedFoodServices;

        var selectedRoomServices = roomServiceIds
       .Zip(roomQuantities, (id, qty) => new { ServiceId = id, Quantity = qty })
       .Where(x => x.Quantity > 0)
       .ToList();
        ViewBag.SelectedRoomServices = selectedRoomServices;
        return View();
    }
}
