using System.Globalization;
using System.Text;
using System.Text.Json;
using Hotel.Models;
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

    public IActionResult CreateCheckoutSession(
        string roomId,
        string roomCategory,
        DateOnly checkIn,
        DateOnly checkOut,
        double roomPrice,
        int numberOfDays,
        double roomTotalPrice,
        double total,
        double subtotal,
        double tax,
        string[]? foodCategories,
        string[]? foodNames,
        int[]? foodQuantities,
        double[]? foodPrices,
        string[]? roomCategories,
        string[]? roomNames,
        int[]? roomQuantities,
        double[]? roomPrices)
    {
        TempData["RoomId"] = roomId;
        TempData["RoomCategory"] = roomCategory;
        TempData["CheckIn"] = checkIn.ToString("yyyy-MM-dd");
        TempData["CheckOut"] = checkOut.ToString("yyyy-MM-dd");
        TempData["RoomPrice"] = roomPrice.ToString();
        TempData["NumberOfDays"] = numberOfDays.ToString();
        TempData["RoomTotalPrice"] = roomTotalPrice.ToString();
        TempData["Total"] = total.ToString();
        TempData["Subtotal"] = subtotal.ToString();
        TempData["Tax"] = tax.ToString();

        var isRoomAvailable = !db.Bookings.Where(b => b.RoomID == roomId)
                                          .Any(b => (checkIn < b.CheckOutDate && b.CheckInDate < checkOut));

        if (!isRoomAvailable)
        {
            TempData["Info"] = "sorry. The room has been book";
            return RedirectToAction("RoomPage", "Home");
        }

        if (foodNames != null && foodQuantities != null)
        {
            var foodServices = foodNames.Select((name, i) => new ServiceItem
            {
                category = foodCategories[i],
                serviceName = name,
                quantity = foodQuantities[i],
                price = foodPrices[i]
            }).ToList();

            TempData["FoodServices"] = JsonSerializer.Serialize(foodServices);
        }
        if (roomNames != null && roomQuantities != null)
        {
            var roomServices = roomNames.Select((name, i) => new ServiceItem
            {
                category = roomCategories[i],
                serviceName = name,
                quantity = roomQuantities[i],
                price = roomPrices[i]
            }).ToList();

            TempData["RoomServices"] = JsonSerializer.Serialize(roomServices);
        }

        // get the booking details
        var productList = new List<(string name, string description, long amount, int quantity)>();

        productList.Add((
            $"{roomCategory}",
            $"Check-in: {checkIn:yyyy-MM-dd}\nCheck-out: {checkOut:yyyy-MM-dd}",
            (long)(roomPrice * 100),
            numberOfDays
        ));

        if (foodNames != null && foodQuantities != null)
        {
            for (int i = 0; i < foodNames.Length; i++)
            {
                if (foodQuantities[i] > 0)
                {
                    productList.Add((
                        foodNames[i],
                        $"Category: {foodCategories[i]}",
                        (long)(foodPrices[i] * 100) / foodQuantities[i],
                        foodQuantities[i]
                    ));
                }
            }
        }

        if (roomNames != null && roomQuantities != null)
        {
            for (int i = 0; i < roomNames.Length; i++)
            {
                if (roomQuantities[i] > 0)
                {
                    productList.Add((
                        roomNames[i],
                        $"Category: {roomCategories[i]}",
                        (long)(roomPrices[i] * 100) / roomQuantities[i],
                        roomQuantities[i]
                    ));
                }
            }
        }

        productList.Add(("Tax", "TaxRate : 10%", (long)(tax * 100), 1));

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = "https://localhost:7161/Payment/Success",
            CancelUrl = "https://localhost:7161/Payment/Cancel"
        };

        // show the booking details
        foreach (var item in productList)
        {
            var sessionListItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = item.amount,
                    Currency = "myr",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.name,
                        Description = item.description
                    }
                },
                Quantity = item.quantity
            };
            options.LineItems.Add(sessionListItem);
        }

        var service = new SessionService();
        var session = service.Create(options);

        return Redirect(session.Url);
    }

    public IActionResult Success()
    {
        // Booking Data
        var roomId = TempData["RoomId"]?.ToString();
        var checkIn = TempData["CheckIn"]?.ToString();
        var checkOut = TempData["CheckOut"]?.ToString();
        var total = double.Parse(TempData["Total"]?.ToString());
        var checkInDate = DateOnly.FromDateTime(DateTime.Parse(checkIn));
        var checkOutDate = DateOnly.FromDateTime(DateTime.Parse(checkOut));

        // generate BookingID and ServiceBookingID
        var today = DateTime.Today;
        var todayBookings = db.Bookings
            .Where(b => b.BookingDate.Date == today)
            .OrderByDescending(b => b.BookingID)
            .ToList();
        int sequence = todayBookings.Count + 1;
        var bookingID = $"BOK{sequence:D3}{today:yyyyMMdd}";
        var serviceBookingID = $"SER{sequence:D3}{today:yyyyMMdd}";

        // store the food and room service to a list
        var allServices = new List<ServiceItem>();
        if (TempData["FoodServices"] != null)
        {
            var foodServices = JsonSerializer.Deserialize<List<ServiceItem>>(
                TempData["FoodServices"].ToString());
            allServices.AddRange(foodServices);
        }
        if (TempData["RoomServices"] != null)
        {
            var roomServices = JsonSerializer.Deserialize<List<ServiceItem>>(
                TempData["RoomServices"].ToString());
            allServices.AddRange(roomServices);
        }

        // store the service
        foreach (var service in allServices)
        {
            if (service.quantity > 0)
            {
                var serviceBooking = new ServiceBooking
                {
                    ID = Guid.NewGuid().ToString(),
                    ServiceBookingID = serviceBookingID,
                    ServiceID = db.Services.First(s => s.ServiceName == service.serviceName).ServiceID,
                    Qty = service.quantity
                };
                db.ServiceBooking.Add(serviceBooking);
            }
        }

        // store the booking
        var booking = new Booking
        {
            BookingID = bookingID,
            BookingDate = today,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            TotalAmount = total,
            Status = "Paid",
            UserID = User.FindFirst("UserID")?.Value,
            RoomID = roomId,
            ServiceBookingID = serviceBookingID
        };
        db.Bookings.Add(booking);
        db.SaveChanges();

        return View();
    }

    public IActionResult Cancel()
    {
        ViewBag.availableRoomID = TempData["RoomId"];
        ViewBag.roomCategory = TempData["RoomCategory"];
        ViewBag.checkIn = TempData["CheckIn"];
        ViewBag.checkOut = TempData["CheckOut"];
        ViewBag.roomPrice = double.Parse(TempData["RoomPrice"]?.ToString());
        ViewBag.numberOfDays = int.Parse(TempData["NumberOfDays"]?.ToString());
        ViewBag.roomTotalPrice = double.Parse(TempData["RoomTotalPrice"]?.ToString());
        ViewBag.Total = double.Parse(TempData["Total"]?.ToString());
        ViewBag.Subtotal = double.Parse(TempData["Subtotal"]?.ToString());
        ViewBag.Tax = double.Parse(TempData["Tax"]?.ToString());

        if (TempData["FoodServices"] != null)
        {
            ViewBag.SelectedFoodServices = JsonSerializer.Deserialize<List<ServiceItem>>(
                TempData["FoodServices"].ToString());
        }

        if (TempData["RoomServices"] != null)
        {
            ViewBag.SelectedRoomServices = JsonSerializer.Deserialize<List<ServiceItem>>(
                TempData["RoomServices"].ToString());
        }

        TempData["Info"] = "Payment was cancelled.";
        return View("PaymentPage");
    }
}
