using System.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Spendo.Models;

namespace Spendo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly string connectionString = "Host=ep-frosty-block-a5git3ix.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=tJKyV3nxzh5I;SSL Mode=Require;Trust Server Certificate=true";
        [HttpGet]
        public ActionResult<IList<Transaction[]>> Get()
        {
            List<Transaction> transactions = new List<Transaction>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM transactions"; // Assuming your table is named 'users'
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            transactions.Add(new Transaction
                            {
                                ID = (int)row["ID"],
                                Amount = Decimal.Parse(row["Amount"].ToString()),
                                Note = row["Note"].ToString(),
                                TransactionDate = DateTime.Parse(row["TransactionDate"].ToString()),
                                AccountID = int.Parse(row["AccountID"].ToString())
                            });
                        }
                    }
                }
            }
            return Ok(transactions);
        }
        [HttpPost]
        public IActionResult Post([FromBody] Transaction transaction)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO transactions (Amount, Note, TransactionDate, AccountID) VALUES (@Amount, @Note, @TransactionDate, @AccountID)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Amount", transaction.Amount);
                    command.Parameters.AddWithValue("Note", transaction.Note);
                    command.Parameters.AddWithValue("TransactionDate", transaction.TransactionDate);
                    command.Parameters.AddWithValue("AccountID", transaction.AccountID);
                    command.ExecuteNonQuery();
                }
            }

            return CreatedAtAction(nameof(Get), new { id = transaction.ID }, transaction);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Transaction transaction)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE transactions SET Amount = @Amount, Note = @Note, TransactionDate = @TransactionDate, AccountID = @AccountID WHERE ID = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Amount", transaction.Amount);
                    command.Parameters.AddWithValue("Note", transaction.Note);
                    command.Parameters.AddWithValue("TransactionDate", transaction.TransactionDate);
                    command.Parameters.AddWithValue("AccountID", transaction.AccountID);
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
                string query = "DELETE FROM transactions WHERE ID = @id";
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
