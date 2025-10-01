namespace NoSQLProject.Models
{
	public class LoginModel
	{
		private string _email;
		private string _password;

		public LoginModel() { }

		public LoginModel(string email, string password)
		{
			_email = email;
			_password = password;
		}

		public string Email { get => _email; set => _email = value; }
		public string Password { get => _password; set => _password = value; }
	}
}
