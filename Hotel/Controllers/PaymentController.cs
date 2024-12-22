using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var occupiedRooms = db.Bookings
                                  .Where(b => checkIn < b.CheckOutDate &&
                                         b.CheckInDate < checkOut)
                                  .Select(b => b.RoomID);

        var availableRooms = db.Rooms
           .Include(r => r.Category)
           .Where(r => !occupiedRooms.Contains(r.RoomID))
           .Where(r => r.Category.CategoryID == categoryID)  // 移除 ToList() 提高性能
           .Select(r => new {
               r.RoomID,
               r.Category.PricePerNight,
               r.Category.CategoryName
           })
           .FirstOrDefault();

        if (availableRooms == null)
        {
            TempData["Info"] = "No rooms are available for the selected dates.";
            return RedirectToAction("RoomDetailsPage", "Home", new
            {
                CategoryID = categoryID,
                CheckInDate = checkIn.ToString("yyyy-MM-dd"),
                CheckOutDate = checkOut.ToString("yyyy-MM-dd"),
            });
        }

        ViewBag.categoryID = categoryID;
        ViewBag.checkIn = checkIn;
        ViewBag.checkOut = checkOut;

        int numberOfDays = checkOut.DayNumber - checkIn.DayNumber;
        double roomTotalPrice = availableRooms.PricePerNight * numberOfDays;
        ViewBag.availableRoomID = availableRooms.RoomID;
        ViewBag.numberOfDays = numberOfDays;
        ViewBag.roomCategory = availableRooms.CategoryName;
        ViewBag.roomPrice = availableRooms.PricePerNight;
        ViewBag.roomTotalPrice = roomTotalPrice;

        var selectedFoodServices = foodServiceIds
       .Zip(foodQuantities, (id, qty) => new { ServiceId = id, Quantity = qty })
       .Where(x => x.Quantity > 0)
       .Select(x =>
            {
                var foodService = db.Services.FirstOrDefault(fs => fs.ServiceID == x.ServiceId);
                return new
                {
                    serviceName = foodService?.ServiceName,
                    category = foodService?.Category,
                    quantity = x.Quantity,
                    unitPrice = foodService?.UnitPrice,
                    price = x.Quantity * (foodService?.UnitPrice)
                };
            })
       .ToList();
        ViewBag.SelectedFoodServices = selectedFoodServices;

        var selectedRoomServices = roomServiceIds
       .Zip(roomQuantities, (id, qty) => new { ServiceId = id, Quantity = qty })
       .Where(x => x.Quantity > 0)
       .Select(x =>
       {
           var foodService = db.Services.FirstOrDefault(fs => fs.ServiceID == x.ServiceId);
           return new
           {
               serviceName = foodService?.ServiceName,
               category = foodService?.Category,
               quantity = x.Quantity,
               unitPrice = foodService?.UnitPrice,
               price = x.Quantity * (foodService?.UnitPrice)
           };
       })
       .ToList();
        ViewBag.SelectedRoomServices = selectedRoomServices;

        double foodServicesSubtotal = selectedFoodServices.Sum(x => x.price ?? 0);
        double roomServicesSubtotal = selectedRoomServices.Sum(x => x.price ?? 0);
        double subtotal = roomTotalPrice + foodServicesSubtotal + roomServicesSubtotal;
        double tax = Math.Round(subtotal * 0.1, 2);
        double total = subtotal + tax;
        ViewBag.Subtotal = subtotal;
        ViewBag.Tax = tax;
        ViewBag.Total = total;

        return View();
    }
}
