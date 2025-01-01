using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models;

// View Models ----------------------------------------------------------------

#nullable disable warnings
public class LoginVM
{
    [StringLength(100)]
    [EmailAddress]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email")]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "The password must be more than 5 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

public class OptVM
{
    [StringLength(100)]
    [EmailAddress]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email")]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(6, ErrorMessage = "OPT must be 6 characters long.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OPT must be exactly 6 digits.")]
    [Display(Name = "OTP")]
    public string Otp { get; set; }
}

public class RegisterVM
{
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(100)]
    [EmailAddress]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(11, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 11 characters.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Phone number can only contain digits.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "The password must be more than 5 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
}

public class UpdatePasswordVM
{
    [StringLength(100, MinimumLength = 5, ErrorMessage = "The password must be more than 5 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
    [Display(Name = "Current Password")]
    public string Current { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "The password must be more than 5 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string New { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "The password must be more than 5 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
    [Compare("New")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }
}

public class UpdateProfileVM
{
    [StringLength(100)]
    public string? Name { get; set; }

    public string? Email { get; set; }

    [StringLength(11, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 11 characters.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Phone number can only contain digits.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    public string? UserImage { get; set; }

    public IFormFile? Photo { get; set; }
}

public class SendEmailVM
{
    [StringLength(100)]
    [EmailAddress]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email")]
    public string Email { get; set; }
}

public class ResetPasswordVM
{
    public string Token { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("NewPassword")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
}

// Search for the available room
public class RoomSearchVM
{
    [Display(Name = "Check In Date")]
    [DataType(DataType.Date)]
    public DateOnly CheckInDate { get; set; }

    [Display(Name = "Check Out Date")]
    [DataType(DataType.Date)]
    public DateOnly CheckOutDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Number of persons must be at least 1.")]
    public int Person { get; set; }
}

public class HomePageVM
{
    public RoomSearchVM SearchVM { get; set; }
}

public class RoomPageVM
{
    public RoomSearchVM SearchVM { get; set; }
}

public class ServiceItem
{
    public string category { get; set; }
    public string serviceName { get; set; }
    public int quantity { get; set; }
    public double price { get; set; }
}

public class RoomDetailsVM
{
    public string CategoryID { get; set; }

    [Display(Name = "Check In Date :")]
    [DataType(DataType.Date)]
    public DateOnly CheckInDate { get; set; }

    [Display(Name = "Check Out Date :")]
    [DataType(DataType.Date)]
    public DateOnly CheckOutDate { get; set; }

    public string[]? FoodServiceIds { get; set; }
    public int[]? FoodQuantities { get; set; }
    public string[]? RoomServiceIds { get; set; }
    public int[]? RoomQuantities { get; set; }
}

public class AddRoomCategoryVM
{
    [StringLength(100)]
    public string categoryName { get; set; }

    [StringLength(100)]
    public string theme { get; set; }

    [Range(12, int.MaxValue, ErrorMessage = "Size must be greater than 11.")]
    public int size { get; set; }

    [RegularExpression(@"^\d+$", ErrorMessage = "Capacity must be a positive number.")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be 1 or more.")]
    public string capacity { get; set; }

    [StringLength(100)]
    public string bed { get; set; }

    [Range(40, 2000, ErrorMessage = "Price must be between 40 and 2000.")]
    public double price { get; set; }

    public string description { get; set; }

    [Required(ErrorMessage = "Please upload at least one photo.")]
    public List<IFormFile>? Photos { get; set; }
}


public class UpdateRoomCategoryVM
{
    public string categoryID { get; set; }

    [Required]
    [StringLength(100)]
    public string categoryName { get; set; }

    [Required]
    [StringLength(100)]
    public string theme { get; set; }

    [Range(12, int.MaxValue, ErrorMessage = "Size must be greater than 11.")]
    public int size { get; set; }

    [RegularExpression(@"^\d+$", ErrorMessage = "Capacity must be a positive number.")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be 1 or more.")]
    public string capacity { get; set; }

    [StringLength(100)]
    public string bed { get; set; }

    [Range(40, 2000, ErrorMessage = "Price must be between 40 and 2000.")]
    public double price { get; set; }

    public string description { get; set; }

    // List of CategoryImage entities for existing images
    public List<CategoryImage>? CategoryImages { get; set; }

    // List of uploaded photos by the user
    public List<IFormFile>? Photos { get; set; }

    // List of image paths marked for removal
    public List<string>? RemoveImagePaths { get; set; } // Add this property to track removed images
}


public class AddRoomVMs
{
    [Required(ErrorMessage = "Room Number is required.")]
    public string RoomNumber { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [DisplayName("Categories")]
    public string CategoryID { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; }
}

public class UpdateRoomVMs
{
    public string RoomID { get; set; }

    [Required(ErrorMessage = "Room Number is required.")]
    public string RoomNumber { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [DisplayName("Categories")]
    public string CategoryID { get; set; }
}

public class AddServiceVM 
{
    public string serviceName { get; set; }
    public double unitPrice { get; set; }
    public string serviceDescription { get; set; }
    public string serviceType { get; set; }
    public string category { get; set; }
    public string Status { get; set; }
}

public class UpdateServiceVM
{
    public string serviceID { get; set; }
    public string serviceName { get; set; }
    public double unitPrice { get; set; }
    public string serviceDescription { get; set; }
    public string serviceType { get; set; }
    public string category { get; set; }
}

public class ViewBookingDetail
{
    public string bookingID { get; set; }
    public string bookingDate { get; set; }
    public string checkInDate { get; set; }
    public string checkOutDate { get; set; }
    public string totalAmount { get; set; }
    public string userName { get; set; }
    public string roomNumber { get; set; }
}