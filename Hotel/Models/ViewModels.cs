﻿using Microsoft.AspNetCore.Mvc;
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

public class HomePageVM
{
    public List<Service> FoodServices { get; set; }
    public List<RoomViewModel> Rooms { get; set; }
}

public class RoomViewModel
{
    // Room Properties
    public string RoomID { get; set; }
    public string RoomNumber { get; set; }
    public string Status { get; set; }

    // Category Properties
    public string CategoryID { get; set; }
    public string CategoryName { get; set; }
    public string Theme { get; set; }
    public int Size { get; set; }
    public string Capacity { get; set; }
    public string Bed { get; set; }
    public string Description { get; set; }
    public double PricePerNight { get; set; }
    public string RoomImage { get; set; }
    public string CategoryStatus { get; set; }
}

public class RoomDetailsVM
{
    public List<Service> FoodServices { get; set; }
    public List<Service> RoomServices { get; set; }
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

public class AddRoomVM { 
}