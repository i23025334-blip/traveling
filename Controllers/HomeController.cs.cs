using Microsoft.AspNetCore.Mvc;
using Travel.Models;

namespace Travel.Controllers
{
	public class HomeController : Controller
	{
		// === SHOW LOGIN PAGE ===
		public IActionResult Login()
		{
			return View();
		}

		// === HANDLE LOGIN POST ===
		[HttpPost]
		public IActionResult Login(LoginViewModel model)
		{
			DB db = new DB();
			db.Connect();

			if (db.AuthenticateUser(model.Username, model.Password))
			{
				HttpContext.Session.SetString("User", model.Username);
				db.Disconnect();
				return RedirectToAction("Index");
			}

			db.Disconnect();
			ViewBag.ErrorMessage = "Invalid username or password!";
			return View();
		}

		// === LOGOUT ===
		public IActionResult Logout()
		{
			HttpContext.Session.Remove("User");
			return RedirectToAction("Index");
		}

		// === REGISTER PAGE ===
		public IActionResult Register()
		{
			return View();
		}

		// === HANDLE REGISTRATION ===
		[HttpPost]
		public IActionResult Register(RegisterViewModel model)
		{
			DB db = new DB();
			db.Connect();

			bool success = db.RegisterUser(model.Username, model.Password);
			db.Disconnect();

			if (!success)
			{
				ViewBag.ErrorMessage = "Username already exists!";
				return View();
			}

			return RedirectToAction("Login");
		}
	}
}
