using System.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Spendo.Models;

namespace Spendo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly string connectionString = "Host=ep-frosty-block-a5git3ix.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=tJKyV3nxzh5I;SSL Mode=Require;Trust Server Certificate=true";
        [HttpGet]
        public ActionResult<IList<Category[]>> Get()
        {
            List<Category> categories = new List<Category>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM categories"; // Assuming your table is named 'users'
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            categories.Add(new Category
                            {
                                ID = (int)row["ID"],
                                Name = row["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return Ok(categories);
        }
        [HttpPost]
        public IActionResult Post([FromBody] Category category)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO categories (Name) VALUES (@Name)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Name", category.Name);
                    command.ExecuteNonQuery();
                }
            }

            return CreatedAtAction(nameof(Get), new { id = category.ID }, category);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Category category)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE categories SET name = @name WHERE ID = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("name", category.Name);
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
                string query = "DELETE FROM categories WHERE id = @id";
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
    }
}
