using Microsoft.AspNetCore.Mvc;
using Travel.Models;

public class AccountController : Controller
{
	private readonly DB _db = new DB(); // Assuming connection setup is handled in DB

	[HttpGet]
	public IActionResult Login()
	{
		return View();
	}

	[HttpPost]
	public IActionResult Login(LoginViewModel model)
	{
		if (ModelState.IsValid)
		{
			if (_db.Connect())
			{
				if (_db.ValidateUser(model.Email, model.Passwd))
				{
					// Logic for successful login, e.g., setting session variables
					return RedirectToAction("Index", "Home");
				}
				else
				{
					ViewBag.ErrorMessage = "Invalid email or password.";
				}
			}
			else
			{
				ViewBag.ErrorMessage = "Database connection error.";
			}
		}

		return View(model);
	}

	// Registration action
	[HttpPost]
	public IActionResult Register(RegisterViewModel model)
	{
		if (ModelState.IsValid)
		{
			string errorMessage = null;
			if (_db.CreateUser(model.Email, model.Passwd, out errorMessage))
			{
				return RedirectToAction("Login");
			}

			ViewBag.ErrorMessage = errorMessage;
		}
		return View(model); // Return to registration with error message
	}
}
