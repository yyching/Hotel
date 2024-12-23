using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Hotel.Controllers;

public class PaymentController : Controller
{
    private readonly DB db;
    private readonly StripeSettings _stripeSettings;

    public PaymentController(DB db, IOptions<StripeSettings> stripeSettings)
    {
        this.db = db;
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
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
           .Where(r => r.Category.CategoryID == categoryID)
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

    public IActionResult CreateCheckoutSession(double total)
    {

        var currency = "usd"; // Currency code
        var successUrl = "https://localhost:7161/Payment/Success";
        var cancelUrl = "https://localhost:7161/Payment/Cancel";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string>
                {
                    "card"
                },
            LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = (long)(total * 100),  // Amount in smallest currency unit (e.g., cents)
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Product Name",
                                Description = "Product Description"
                            }
                        },
                        Quantity = 1
                    }
                },
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        var service = new SessionService();
        var session = service.Create(options);

        return Redirect(session.Url);
    }
}
