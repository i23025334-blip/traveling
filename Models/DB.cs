using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;   // use Microsoft.Data.SqlClient
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Travel.Models
{
	public class DB
	{
		private SqlConnection connection = null;

		private string GetConnectionString()
		{
			string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "travel.db.mdf");
			return $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;Connect Timeout=30;";
		}

		public bool Connect()
		{
			string connectionString = GetConnectionString();
			connection = new SqlConnection(connectionString);
			try
			{
				connection.Open();
				return connection.State == ConnectionState.Open;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Connection error: " + ex.Message);
				return false;
			}
		}

		public void Disconnect()
		{
			if (connection != null && connection.State != ConnectionState.Closed)
			{
				connection.Close();
				connection.Dispose();
			}
		}

		// ---------- User methods ----------
		// Simple salted password hashing using PBKDF2
		private string HashPassword(string password)
		{
			byte[] salt;
			byte[] buffer2;
			using (var rng = RandomNumberGenerator.Create())
			{
				salt = new byte[16];
				rng.GetBytes(salt);
			}
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
			{
				buffer2 = pbkdf2.GetBytes(32);
			}

			var dst = new byte[1 + 16 + 32];
			dst[0] = 0x00; // version
			Buffer.BlockCopy(salt, 0, dst, 1, 16);
			Buffer.BlockCopy(buffer2, 0, dst, 17, 32);
			return Convert.ToBase64String(dst);
		}

		private bool VerifyHashedPassword(string hashedPassword, string password)
		{
			if (hashedPassword == null) return false;
			var src = Convert.FromBase64String(hashedPassword);
			if (src.Length != 1 + 16 + 32) return false;
			var salt = new byte[16];
			Buffer.BlockCopy(src, 1, salt, 0, 16);
			var storedSubkey = new byte[32];
			Buffer.BlockCopy(src, 17, storedSubkey, 0, 32);

			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
			{
				var generatedSubkey = pbkdf2.GetBytes(32);
				return CryptographicOperations.FixedTimeEquals(storedSubkey, generatedSubkey);
			}
		}

		public bool CreateUser(string username, string email, string password, out string errorMessage)
		{
			errorMessage = null;
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				errorMessage = "Username and password required.";
				return false;
			}

			try
			{
				string insertSql = "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@username, @email, @passwordHash)";
				using (var cmd = new SqlCommand(insertSql, connection))
				{
					cmd.Parameters.AddWithValue("@username", username);
					cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@passwordHash", HashPassword(password));
					cmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (SqlException ex) when (ex.Number == 2627) // unique constraint
			{
				errorMessage = "Username already exists.";
				return false;
			}
			catch (Exception ex)
			{
				errorMessage = "DB error: " + ex.Message;
				return false;
			}
		}

		public UserRecord GetUserByUsername(string username)
		{
			string sql = "SELECT Id, Username, Email, PasswordHash FROM Users WHERE Username = @username";
			using (var cmd = new SqlCommand(sql, connection))
			{
				cmd.Parameters.AddWithValue("@username", username);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new UserRecord
						{
							Id = Convert.ToInt32(reader["Id"]),
							Username = reader["Username"].ToString(),
							Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
							PasswordHash = reader["PasswordHash"].ToString()
						};
					}
				}
			}
			return null;
		}

		public bool ValidateUser(string username, string password)
		{
			var user = GetUserByUsername(username);
			if (user == null) return false;
			return VerifyHashedPassword(user.PasswordHash, password);
		}

		// Add a small DTO for user
		public class UserRecord
		{
			public int Id { get; set; }
			public string Username { get; set; }
			public string Email { get; set; }
			public string PasswordHash { get; set; }
		}

		// Keep your product CRUD below unchanged...
		// (copy in your existing product methods here)
		public void Create(Product product)
		{
			string query = "INSERT INTO Product (name, price, quantity, unit, origin) " +
						   "VALUES (@name, @price, @quantity, @unit, @origin)";

			using (SqlCommand cmd = new SqlCommand(query, connection))
			{
				cmd.Parameters.AddWithValue("@name", product.name);
				cmd.Parameters.AddWithValue("@price", product.price);
				cmd.Parameters.AddWithValue("@quantity", product.quantity);
				cmd.Parameters.AddWithValue("@unit", product.unit);
				cmd.Parameters.AddWithValue("@origin", product.origin);

				cmd.ExecuteNonQuery();
			}
		}

		public void Update(Product product)
		{
			string query = "UPDATE Product SET name=@name, price=@price, quantity=@quantity, " +
						   "unit=@unit, origin=@origin WHERE id=@id";

			using (SqlCommand cmd = new SqlCommand(query, connection))
			{
				cmd.Parameters.AddWithValue("@name", product.name);
				cmd.Parameters.AddWithValue("@price", product.price);
				cmd.Parameters.AddWithValue("@quantity", product.quantity);
				cmd.Parameters.AddWithValue("@unit", product.unit);
				cmd.Parameters.AddWithValue("@origin", product.origin);
				cmd.Parameters.AddWithValue("@id", product.id);

				cmd.ExecuteNonQuery();
			}
		}

		public void Delete(int id)
		{
			string query = "DELETE FROM Product WHERE id=@id";

			using (SqlCommand cmd = new SqlCommand(query, connection))
			{
				cmd.Parameters.AddWithValue("@id", id);
				cmd.ExecuteNonQuery();
			}
		}

		public List<Product> Read()
		{
			List<Product> list = new List<Product>();
			string query = "SELECT * FROM Product";

			using (SqlCommand cmd = new SqlCommand(query, connection))
			using (SqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					Product p = new Product
					{
						id = Convert.ToInt32(reader["id"]),
						name = reader["name"].ToString(),
						price = Convert.ToDecimal(reader["price"]),
						quantity = Convert.ToInt32(reader["quantity"]),
						unit = reader["unit"].ToString(),
						origin = reader["origin"].ToString()
					};

					list.Add(p);
				}
			}

			return list;
		}

		public Product Read(int id)
		{
			Product product = null;
			string query = "SELECT * FROM Product WHERE id=@id";

			using (SqlCommand cmd = new SqlCommand(query, connection))
			{
				cmd.Parameters.AddWithValue("@id", id);

				using (SqlDataReader reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						product = new Product
						{
							id = Convert.ToInt32(reader["id"]),
							name = reader["name"].ToString(),
							price = Convert.ToDecimal(reader["price"]),
							quantity = Convert.ToInt32(reader["quantity"]),
							unit = reader["unit"].ToString(),
							origin = reader["origin"].ToString()
						};
					}
				}
			}

			return product;
		}

	}
}
