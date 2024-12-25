using Dapper;
using Npgsql;
using Spendo;
using System.Data;

namespace Spendo;

/// <summary>
/// Context used to create a database connection and execute 
/// SQL commands against the DB.
/// </summary>
internal class NeonContext
{
    private NeonConfig _config;

    public NeonContext(NeonConfig config)
    {
        _config = config;
    }
    public NeonContext() { }

    /// <summary>
    /// Creates connection object used to interact with the database.
    /// </summary>
    /// <returns></returns>
    private IDbConnection CreateConnection()
    {
        var connectionString = "Host=ep-frosty-block-a5git3ix.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=tJKyV3nxzh5I;SSL Mode=Require;Trust Server Certificate=true";
        return new NpgsqlConnection(connectionString);
    }

    /// <summary>
    /// Instantiates a 
    /// </summary>
    /// <returns></returns>
    public async Task Init()
    {
        await _initDatabase();
        await _createTables();
    }

    /// <summary>
    /// Creates the database if it doesn't exist.
    /// </summary>
    /// <returns></returns>
    private async Task _initDatabase()
    {
        if (string.IsNullOrWhiteSpace("neondb"))
        {
            throw new InvalidOperationException("Database name cannot be null, empty, or whitespace.");
        }

        using var connection = CreateConnection();
        var dbCountQuery = $"SELECT COUNT(*) FROM pg_database WHERE datname = 'neondb';";
        var dbCount = await connection.ExecuteScalarAsync<int>(dbCountQuery);
        if (dbCount == 0)
        {
            var createDbQuery = $"CREATE DATABASE \"neondb\"";
            await connection.ExecuteAsync(createDbQuery);
        }
    }


    /// <summary>
    /// Creates the tables in the database.
    /// </summary>
    /// <returns></returns>
    private async Task _createTables()
    {
        await _initUser();
        await _initCategory();
        await _initAccount();
        await _initTransaction();
        await _initSaving();
        await _initIncome();
        await _initOutcome();
    }

    /// <summary>
    /// Create categories table
    /// </summary>
    /// <returns></returns>
    ///
    private async Task _initUser()
    {
        using var connection = CreateConnection();
        var sql = @"
			CREATE TABLE IF NOT EXISTS Users (
				ID SERIAL PRIMARY KEY,
                FullName VARCHAR(255),
                Email VARCHAR(255),
                Password VARCHAR(255),
                CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
			);
		";
        await connection.ExecuteAsync(sql);
    }
    private async Task _initCategory()
    {
        using var connection = CreateConnection();
        var sql = @"
			CREATE TABLE IF NOT EXISTS Categories (
				ID SERIAL PRIMARY KEY,
                Name VARCHAR(255),
                UserID INT,
                CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (UserID) REFERENCES Users(ID)
			);
		";
        await connection.ExecuteAsync(sql);
    }
    private async Task _initTransaction()
    {
        using var connection = CreateConnection();
        var sql = @"
    	CREATE TABLE IF NOT EXISTS Transactions (
    		        ID SERIAL PRIMARY KEY,
                  Amount DECIMAL(10,2),
                  Note TEXT,
                  TransactionDate DATE,
                  AccountID INT,
                  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY (AccountID) REFERENCES Accounts(ID)
    	);
    ";
        await connection.ExecuteAsync(sql);
    }

    private async Task _initSaving()
    {
        using var connection = CreateConnection();
        var sql = @"
    	CREATE TABLE IF NOT EXISTS Savings (
    		ID SERIAL PRIMARY KEY,
                  Title VARCHAR(255),
                  Goal TEXT,
                  SavingDate DATE,
                  Amount DECIMAL(10,2),
                  Status VARCHAR(50),
                  UserID INT,
                  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY (UserID) REFERENCES Users(ID)
    	);
    ";
        await connection.ExecuteAsync(sql);
    }

    private async Task _initAccount()
    {
        using var connection = CreateConnection();
        var sql = @"
    	CREATE TABLE IF NOT EXISTS Accounts (
    		ID SERIAL PRIMARY KEY,
                  Balance DECIMAL(10,2),
                  UserID INT,
                  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY (UserID) REFERENCES Users(ID)
    	);
    ";
        await connection.ExecuteAsync(sql);
    }
    private async Task _initIncome()
    {
        using var connection = CreateConnection();
        var sql = @"
    	CREATE TABLE IF NOT EXISTS Incomes (
    		ID SERIAL PRIMARY KEY,
                  Title VARCHAR(255),
                  Money DECIMAL(10,2),
                  Description TEXT,
                  CategoryID INT,
                  AccountID INT,
                  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY (CategoryID) REFERENCES Categories(ID),
                  FOREIGN KEY (AccountID) REFERENCES Accounts(ID)
    	);
    ";
        await connection.ExecuteAsync(sql);
    }
    private async Task _initOutcome()
    {
        using var connection = CreateConnection();
        var sql = @"
    	CREATE TABLE IF NOT EXISTS Outcomes (
    		ID SERIAL PRIMARY KEY,
                  Title VARCHAR(255),
                  Money DECIMAL(10,2),
                  Description TEXT,
                  CategoryID INT,
                  AccountID INT,
                  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                  FOREIGN KEY (CategoryID) REFERENCES Categories(ID),
                  FOREIGN KEY (AccountID) REFERENCES Accounts(ID)
    	);
    ";
        await connection.ExecuteAsync(sql);
    }

}