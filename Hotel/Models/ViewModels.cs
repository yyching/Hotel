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
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

public class RegisterVM
{
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(100)]
    [EmailAddress]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(11, MinimumLength = 10)]
    [RegularExpression(@"^\d{10,11}$")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, MinimumLength = 5)]
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
    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string Current { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string New { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("New")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }

    public string? UserImage { get; set; }
    public string Name { get; set; }
}

public class UpdateProfileVM
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(11, MinimumLength = 10)]
    [RegularExpression(@"^\d{10,11}$")]
    public string? PhoneNumber { get; set; }

    public string? UserImage { get; set; }

    public IFormFile? Photo { get; set; }
}

public class SendEmailVM
{
    [StringLength(100)]
    [EmailAddress]
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
    public List<Service> FoodServices { get; set; }
    public List<Category> Categories { get; set; }
    public RoomSearchVM SearchVM { get; set; }
}

public class RoomPageVM
{
    public List<Category> Categories { get; set; }
    public RoomSearchVM SearchVM { get; set; }
}

public class RoomDetailsVM
{

}

public class AddRoomCategoryVM 
{
    [StringLength(100)]
    public string categoryName { get; set; }

    [StringLength(100)]
    public string theme { get; set; }

    public int size { get; set; }

    [StringLength(100)]
    public string capacity { get; set; }

    [StringLength(100)]
    public string bed { get; set; }

    public double price { get; set; }

    public string description { get; set; }

    public IFormFile? Photo { get; set; }
}

public class UpdateRoomCategoryVM
{
    public string categoryID { get; set; }

    [StringLength(100)]
    public string categoryName { get; set; }

    [StringLength(100)]
    public string theme { get; set; }

    public int size { get; set; }

    [StringLength(100)]
    public string capacity { get; set; }

    [StringLength(100)]
    public string bed { get; set; }

    public double price { get; set; }

    public string description { get; set; }

    public string? roomImage { get; set; }

    public IFormFile? Photo { get; set; }
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