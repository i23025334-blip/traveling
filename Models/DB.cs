using System;
using System.Collections.Generic;
using System.Data.SqlClient;
//using Microsoft.Data.SqlClient;

namespace Travel.Models
{
	public class DB
	{
		private SqlConnection connection = null;

		public bool Connect()
		{
			string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "travel.db.mdf");
			string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;";

			connection = new SqlConnection(connectionString);

			try
			{
				connection.Open();
				return connection.State == System.Data.ConnectionState.Open;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Connection error: " + ex.Message);
				return false;
			}
		}

		public void Disconnect()
		{
			if (connection != null && connection.State != System.Data.ConnectionState.Closed)
				connection.Close();
		}

		// --- LOGIN USING DATABASE TABLE "Users" ----
		public bool AuthenticateUser(string username, string password)
		{
			string query = "SELECT COUNT(*) FROM Users WHERE Username=@u AND Password=@p";

			using (SqlCommand cmd = new SqlCommand(query, connection))
			{
				cmd.Parameters.AddWithValue("@u", username);
				cmd.Parameters.AddWithValue("@p", password);

				int count = (int)cmd.ExecuteScalar();
				return count > 0;
			}
		}

		// --- REGISTER USER ---
		public bool RegisterUser(string username, string password)
		{
			string check = "SELECT COUNT(*) FROM Users WHERE Username=@u";
			using (SqlCommand cmd = new SqlCommand(check, connection))
			{
				cmd.Parameters.AddWithValue("@u", username);
				int exists = (int)cmd.ExecuteScalar();

				if (exists > 0)
					return false;
			}

			string insert = "INSERT INTO Users (Username, Password) VALUES (@u, @p)";
			using (SqlCommand cmd = new SqlCommand(insert, connection))
			{
				cmd.Parameters.AddWithValue("@u", username);
				cmd.Parameters.AddWithValue("@p", password);
				cmd.ExecuteNonQuery();
			}

			return true;
		}
	}
}
