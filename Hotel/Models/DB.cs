using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Models;

public class DB : DbContext
{
    public DB(DbContextOptions<DB> options) : base(options) { }

    // DbSets for each entity
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<Service> Services { get; set; }
}

public class User
{
    // Column
    [Key, MaxLength(10)]
    public string UserID { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(100), EmailAddress]
    public string Email { get; set; }
    [MaxLength(100)]
    public string Password { get; set; }
    [MaxLength(15)]
    public string PhoneNumber { get; set; }
    [MaxLength(50)]
    public string Role { get; set; }
    public string UserImage { get; set; }

    // Navigation
    public List<Review> Reviews { get; set; }
    public List<Booking> Bookings { get; set; }
}

public class Category
{
    // Column
    [Key, MaxLength(10)]
    public string CategoryID { get; set; }
    [MaxLength(50)]
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public string RoomImage { get; set; }

    // Navigation
    public List<Room> Rooms { get; set; }
}

public class Room
{
    // Column
    [Key, MaxLength(10)]
    public string RoomID { get; set; }
    [MaxLength(10)]
    public string RoomNumber { get; set; }
    public double PricePerNight { get; set; }
    [MaxLength(15)]
    public string AvailabilityStatus { get; set; }

    // Foreign Key
    public string CategoryID { get; set; }

    // Navigation
    public Category Category { get; set; }
    public List<Booking> Bookings { get; set; }
}

public class Review
{
    // Column
    [Key, MaxLength(10)]
    public string ReviewID { get; set; }
    public int RatingValue { get; set; }
    public string ReviewText { get; set; }
    public DateTime ReviewDate { get; set; }

    // Foreign Key
    public string UserID { get; set; }

    // Navigation
    public User User { get; set; }
}

public class Booking
{
    // Column
    [Key, MaxLength(10)]
    public string BookingID { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public double TotalAmount { get; set; }

    // Foreign Key
    public string UserID { get; set; }
    public string RoomID { get; set; }
    public string? ServiceID { get; set; }

    // Navigation
    public User User { get; set; }
    public Room Room { get; set; }
    public Service Service { get; set; }
}

public class ServiceType
{
    // Column
    [Key, MaxLength(10)]
    public string ServiceTypeID { get; set; }
    [MaxLength(50)]
    public string ServiceTypeName { get; set; }

    // Navigation
    public List<Service> Services { get; set; }
}

public class Service
{
    // Column
    [Key, MaxLength(10)]
    public string ServiceID { get; set; }
    [MaxLength(100)]
    public string ServiceName { get; set; }
    public int Qty { get; set; }
    public double UnitPrice { get; set; }
    public string ServiceImage { get; set; }
    public string ServiceDescription { get; set; }

    // Foreign Key
    public string ServiceTypeID { get; set; }

    // Navigation
    public ServiceType ServiceType { get; set; }
}
