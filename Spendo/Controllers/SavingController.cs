using System.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Spendo.Models;

namespace Spendo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SavingController : ControllerBase
    {
        private readonly string connectionString = "Host=ep-frosty-block-a5git3ix.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=tJKyV3nxzh5I;SSL Mode=Require;Trust Server Certificate=true";
        [HttpGet]
        public ActionResult<IList<Saving[]>> Get()
        {
            List<Saving> savings = new List<Saving>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM savings"; // Assuming your table is named 'users'
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            savings.Add(new Saving
                            {
                                ID = (int)row["ID"],
                                Title = row["Title"].ToString(),
                                Amount = Decimal.Parse(row["Amount"].ToString()),
                                Goal = Decimal.Parse(row["Goal"].ToString()),
                                SavingDate = DateTime.Parse(row["DateTime"].ToString()),
                                Status = row["Status"].ToString(),
                                UserID = int.Parse(row["ID"].ToString())
       

                            });
                        }
                    }
                }
            }
            return Ok(savings);
        }
        [HttpPost]
        public IActionResult Post([FromBody] Saving saving)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO savings (Title, Amount, Goal, SavingDate, Status, UserID) VALUES (@Title, @Amount, @Goal, @SavingDate, @Status, @UserID)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Title", saving.Title);
                    command.Parameters.AddWithValue("Amount", saving.Amount);
                    command.Parameters.AddWithValue("Goal", saving.Goal);
                    command.Parameters.AddWithValue("SavingDate", saving.SavingDate);
                    command.Parameters.AddWithValue("Status", saving.Status);
                    command.Parameters.AddWithValue("UserID", saving.UserID);
                    command.ExecuteNonQuery();
                }
            }

            return CreatedAtAction(nameof(Get), new { id = saving.ID }, saving);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Saving saving)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE savings SET title = @title, amount = @amount, goal = @goal, status = @status, userID = @userID WHERE ID = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("title", saving. Title);
                    command.Parameters.AddWithValue("amount", saving. Amount);
                    command.Parameters.AddWithValue("goal", saving. Goal);
                    command.Parameters.AddWithValue("status", saving. Status);
                    command.Parameters.AddWithValue("userID", saving. UserID);
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
                string query = "DELETE FROM savings WHERE ID = @id";
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
