using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;

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

		// ---------------- USER MANAGEMENT (PLAIN TEXT PASSWORD) ----------------
		/*
		public bool CreateUser(string username, string email, string passwd, out string errorMessage)
		{
			errorMessage = null;

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwd))
			{
				errorMessage = "Username and password required.";
				return false;
			}

			try
			{
				string insertSql = "INSERT INTO Users (Username, Email, Passwd, IsVerified, IsAdmin) " +
								   "VALUES (@username, @email, @passwd, 0, 0)";

				using (var cmd = new SqlCommand(insertSql, connection))
				{
					cmd.Parameters.AddWithValue("@username", username);
					cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@passwd", passwd);   // plain text

					cmd.ExecuteNonQuery();
				}

				return true;
			}
			catch (SqlException ex) when (ex.Number == 2627)
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
		*/
		public bool CreateUser(string email, string passwd, out string errorMessage)
		{
			errorMessage = null;

			// Check for empty fields
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwd))
			{
				errorMessage = "Email and password are required.";
				return false;
			}

			try
			{
				string insertSql = "INSERT INTO Users (Email, Passwd, IsVerified, IsAdmin) VALUES (@Email, @Passwd, 0, 0)";

				using (var cmd = new SqlCommand(insertSql, connection))
				{
					cmd.Parameters.AddWithValue("@Email", email);
					cmd.Parameters.AddWithValue("@Passwd", passwd); // Store in plain text (not recommended)

					cmd.ExecuteNonQuery();
				}

				return true;
			}
			catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
			{
				errorMessage = "Email already exists.";
				return false;
			}
			catch (Exception ex)
			{
				errorMessage = "Database error: " + ex.Message;
				return false;
			}
		}

		public UserRecord GetUserByEmail(string email)
		{
			string sql = "SELECT Id, Username, Email, Passwd, IsVerified, IsAdmin " +
						 "FROM Users WHERE Email = @Email";  // Search by email

			using (var cmd = new SqlCommand(sql, connection))
			{
				cmd.Parameters.AddWithValue("@Email", email);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new UserRecord
						{
							Id = Convert.ToInt32(reader["Id"]),
							Username = reader["Username"].ToString(),
							Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
							Passwd = reader["Passwd"].ToString(),
							IsVerified = Convert.ToBoolean(reader["IsVerified"]),
							IsAdmin = Convert.ToBoolean(reader["IsAdmin"])
						};
					}
				}
			}

			return null;
		}

		public bool ValidateUser(string email, string password)
		{
			var user = GetUserByEmail(email);  // Use email instead of username
			if (user == null)
				return false;

			return user.Passwd == password;  // Plain text check (not secure)
		}


		public class UserRecord
		{
			public int Id { get; set; }
			public string Username { get; set; }
			public string Email { get; set; }
			public string Passwd { get; set; }
			public bool IsVerified { get; set; }
			public bool IsAdmin { get; set; }
		}

		// ---------------- PRODUCT CRUD (UNCHANGED) ----------------

		public void Create(Product product)
		{
			string query = "INSERT INTO Product (name, price, quantity, unit, origin) VALUES (@name, @price, @quantity, @unit, @origin)";
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
			string query = "UPDATE Product SET name=@name, price=@price, quantity=@quantity, unit=@unit, origin=@origin WHERE id=@id";
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
					list.Add(new Product
					{
						id = Convert.ToInt32(reader["id"]),
						name = reader["name"].ToString(),
						price = Convert.ToDecimal(reader["price"]),
						quantity = Convert.ToInt32(reader["quantity"]),
						unit = reader["unit"].ToString(),
						origin = reader["origin"].ToString()
					});
				}
			}
			return list;
		}

		public Product Read(int id)
		{
			string query = "SELECT * FROM Product WHERE id=@id";

			using (SqlCommand cmd = new SqlCommand(query, connection))
			{
				cmd.Parameters.AddWithValue("@id", id);

				using (SqlDataReader reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new Product
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

			return null;
		}
	}
}
