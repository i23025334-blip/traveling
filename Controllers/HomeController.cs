using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Travel.Models;
using Microsoft.AspNetCore.Http;

namespace Travel.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy() => View();

		// GET: Login
		[HttpGet]
		public IActionResult Login()
		{
			return View(new LoginViewModel());
		}

		// POST: Login
		[HttpPost]
		public IActionResult Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var db = new DB();
			if (!db.Connect())
			{
				ViewBag.ErrorMessage = "Database connection failed.";
				return View(model);
			}

			bool ok = db.ValidateUser(model.Email, model.Passwd);
			db.Disconnect();

			if (ok)
			{
				// Set a simple session value to indicate logged-in user
				HttpContext.Session.SetString("Email", model.Email);
				return RedirectToAction("Index");
			}
			ViewBag.ErrorMessage = "Invalid username or password.";
			return View(model);
		}

		// GET: Register
		[HttpGet]
		public IActionResult Register()
		{
			return View(new LoginViewModel());
		}

		// POST: Register
		[HttpPost]
		public IActionResult Register(LoginViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var db = new DB();
			if (!db.Connect())
			{
				ViewBag.ErrorMessage = "Database connection failed.";
				return View(model);
			}

			string error;
			bool created = db.CreateUser(model.Email, model.Email, model.Passwd, out error);
			db.Disconnect();

			if (!created)
			{
				ViewBag.ErrorMessage = error;
				return View(model);
			}

			// auto-login after register
			HttpContext.Session.SetString("Username", model.Email);
			return RedirectToAction("Index");
		}

		public IActionResult Logout()
		{
			HttpContext.Session.Remove("Username");
			return RedirectToAction("Index");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
