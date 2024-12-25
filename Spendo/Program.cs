namespace Spendo;

public class Program
{
    public static async Task Main(string[] args) // Đổi void thành Task
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Đọc cấu hình từ appsettings.json
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // 2. Instantiate a new NeonConfig object and bind
        //    properties from NeonConnection to it
        var neonConfig = new NeonConfig();
        configuration.GetSection("NeonConnection").Bind(neonConfig);

        // 3. Instantiate a new NeonContext, passing in the 
        //    above config. This is how we access the DB.
        var connection = new NeonContext(neonConfig);

        // 4. Initiate the connection and create the tables!
        await connection.Init(); // Đây là lý do cần async Task

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
