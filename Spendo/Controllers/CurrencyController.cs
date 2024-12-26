using System.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Spendo.Models;

namespace Spendo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly string connectionString = "Host=ep-frosty-block-a5git3ix.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=tJKyV3nxzh5I;SSL Mode=Require;Trust Server Certificate=true";
        [HttpGet]
        public ActionResult<IList<Account[]>> Get()
        {
            List<Currency> currencies = new List<Currency>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM currencies"; // Assuming your table is named 'users'
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            currencies.Add(new Currency
                            {
                                ID = (int)row["ID"],
                                Name = row["Name"].ToString(),
                                Code = row["Code"].ToString(),
                                Sign = row["Sign"].ToString(),
                            });
                        }
                    }
                }
            }
            return Ok(currencies);
        }
        [HttpPost]
        public IActionResult Post([FromBody] Currency currency)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO currencies (Name, Code, Sign) VALUES (@Name, @Code, @Sign)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Name", currency.Name);
                    command.Parameters.AddWithValue("Code", currency.Code);
                    command.Parameters.AddWithValue("Sign", currency.Sign);
                    command.ExecuteNonQuery();
                }
            }

            return CreatedAtAction(nameof(Get), new { id = currency.ID }, currency);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Currency currency)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE currencies SET Name = @Name, Code = @Code, Sign = @Sign WHERE ID = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Name", currency.Name);
                    command.Parameters.AddWithValue("Code", currency.Code);
                    command.Parameters.AddWithValue("Sign", currency.Sign);
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
                string query = "DELETE FROM currencies WHERE id = @id";
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
