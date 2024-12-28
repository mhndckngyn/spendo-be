using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Spendo.Models;

namespace Spendo.Controllers
{
    [Route("[controller]")]
    [ApiController]


    public class UserController : ControllerBase
    {
        private readonly string connectionString = "Host=ep-frosty-block-a5git3ix.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=tJKyV3nxzh5I;SSL Mode=Require;Trust Server Certificate=true";
        [HttpGet]
        public ActionResult<IList<User[]>> Get()
        {
            List<User> users = new List<User>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM users"; // Assuming your table is named 'users'
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            users.Add(new User
                            {
                                ID = (int)row["ID"],
                                FullName = row["FullName"].ToString(),
                                Email = row["Email"].ToString(),
                                Password = row["Password"].ToString()
                            });
                        }
                    }
                }
            }
            return Ok(users);
        }
        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {
            if (IsExist(user.Email))
            {
                return Conflict();
            }
            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO users (FullName, Email, Password) VALUES (@FullName, @Email, @Password)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("FullName", user.FullName);
                    command.Parameters.AddWithValue("Email", user.Email);
                    command.Parameters.AddWithValue("Password", user.Password);
                    command.ExecuteNonQuery();
                }
            }

            return CreatedAtAction(nameof(Get), new { id = user.ID }, user);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] User user)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET name = @name, email = @email, password = @password WHERE ID = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("name", user.FullName);
                    command.Parameters.AddWithValue("email", user.Email);
                    command.Parameters.AddWithValue("password", user.Password);
                    command.Parameters.AddWithValue("id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(); // If no rows were affected, return 404
                    }
                }
            }

            return NoContent(); // Successfully updated
        }

        // Delete a user (Delete)
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM users WHERE id = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(); // If no rows were affected, return 404
                    }
                }
            }

            return NoContent(); // Successfully deleted
        }
        public bool IsExist(string email)
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM users WHERE email = @email";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("email", email);
            return (command.ExecuteScalar() != null);
        }
    }
}
