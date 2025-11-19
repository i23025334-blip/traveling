namespace Travel.Models
{
	public class LoginViewModel
	{
		// use PascalCase to match model binding conventions
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; } // for register
	}
}
