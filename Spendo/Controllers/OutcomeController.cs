using System.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Spendo.Models;

namespace Spendo.Controllers
{
    public class OutcomeController : ControllerBase
    {
        private readonly string connectionString = "Host=ep-frosty-block-a5git3ix.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=tJKyV3nxzh5I;SSL Mode=Require;Trust Server Certificate=true";
        [HttpGet]
        public ActionResult<IList<Outcome[]>> Get()
        {
            List<Outcome> outcomes = new List<Outcome>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM outcomes"; // Assuming your table is named 'users'
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            outcomes.Add(new Outcome
                            {
                                ID = (int)row["ID"],
                                Money = Decimal.Parse(row["Money"].ToString()),
                                AccountId = int.Parse(row["AccountId"].ToString()),
                                CategoryId = int.Parse(row["CategoryId"].ToString()),
                                Title = row["Title"].ToString(),
                                Description = row["Description"].ToString()
                            });
                        }
                    }
                }
            }
            return Ok(outcomes);
        }
        [HttpPost]
        public IActionResult Post([FromBody] Outcome outcome)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO outcomes (Money, Title, AccountId, CategoryId, Description) VALUES (@Money, @Title, @AccountId, @CategoryId, @Description)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Money", outcome.Money);
                    command.Parameters.AddWithValue("Title", outcome.Title);
                    command.Parameters.AddWithValue("AccountId", outcome.AccountId);
                    command.Parameters.AddWithValue("CategoryId", outcome.CategoryId);
                    command.Parameters.AddWithValue("Description", outcome.Description);
                    command.ExecuteNonQuery();
                }
            }

            return CreatedAtAction(nameof(Get), new { id = outcome.ID }, outcome);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Outcome outcome)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE outcomes SET Money = @Money, Title = @Title, AccountId = @AccountId, CategoryId = @CategoryId, Description = @Description WHERE ID = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Money", outcome.Money);
                    command.Parameters.AddWithValue("Title", outcome.Title);
                    command.Parameters.AddWithValue("AccountId", outcome.AccountId);
                    command.Parameters.AddWithValue("CategoryId", outcome.CategoryId);
                    command.Parameters.AddWithValue("Description", outcome.Description);
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
                string query = "DELETE FROM outcomes WHERE ID = @id";
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
