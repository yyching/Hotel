﻿using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class HomeController : Controller
{
    private readonly DB db;

    public HomeController(DB db) 
    {  
        this.db = db; 
    }

    //public IActionResult Index(string? ServiceTypeID)
    //{
    //    ViewBag.ServiceTypes = db.ServiceTypes;

    //    var m = db.Services.Where(s => s.ServiceTypeID == ServiceTypeID);

    //    return View(m);
    //}

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Account()
    {
        return View();
    }

    public IActionResult CPassword()
    {
        return View();
    }

    public IActionResult RoomDetailsPage()
    {
        return View();
    }

    public IActionResult RoomPage()
    {
        return View();
    }
}
