﻿using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class ProfileController : Controller
{
    private readonly DB db;

    public ProfileController(DB db)
    {
        this.db = db;
    }

    public IActionResult LoginRegister()
    {
        return View();
    }

    public IActionResult Account()
    {
        return View();
    }
}
