using Microsoft.AspNetCore.Mvc;
using Travel.Models;

namespace Travel.Controllers
{
	public class AccountController : Controller
	{
		private DB db = new DB();  // Instance of your DB class

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
				// Validate user credentials
				if (db.Connect())  // Ensure the connection is successful
				{
					if (db.ValidateUser(model.Email, model.Passwd))
					{
						// Successful login logic (set session, redirect, etc.)
						return RedirectToAction("Index", "Home"); // Redirect to homepage
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

			return View(model);  // Return to login view with the Error
		}

		// Dispose method to close the database connection
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Disconnect();
			}
			base.Dispose(disposing);
		}
	}

}
