using System;
using System.Collections.Generic;
using System.Data.SqlClient;
//using Microsoft.Data.SqlClient;

namespace Travel.Models
{
	public class DB
	{
		private SqlConnection connection = null;
		//private SqlConnection connection;

		public bool Connect()
		{
			string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "travel.db.mdf");
			string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;";

			connection = new SqlConnection(connectionString);
			connection.Open();

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
			{
				connection.Close();
			}
		}

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

		// Simple in-memory authentication
		public class UserService
		{
			private List<LoginViewModel> users = new List<LoginViewModel>
			{
				new LoginViewModel { username = "qwer", password = "qwer" },
				new LoginViewModel { username = "user", password = "1234" }
			};

			public bool AuthenticateUser(string username, string password)
			{
				var user = users.Find(u => u.username == username);
				return user != null && user.password == password;
			}
		}
	}
}
