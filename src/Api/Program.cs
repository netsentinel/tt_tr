using Api.Repositories;
using Api.Services;
using Npgsql;
using Serilog;

namespace Api;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateSlimBuilder(args);

		builder.Configuration
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddJsonFile($"appsettings.json", false, true)
			.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

		builder.Services.AddSerilog((_, configure) => configure
			.ReadFrom.Configuration(builder.Configuration)
		);

		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddScoped<NpgsqlConnection>(
			provider => new NpgsqlConnection(builder.Configuration["ConnectionStrings:DefaultConnection"]));

        builder.Services.AddScoped<BaseRepository>();
        builder.Services.AddScoped<InterviewService>();
        builder.Services.AddScoped<SurveyService>();
        builder.Services.AddScoped<DatabaseService>();

		var app = builder.Build();

		app.UseSwagger();
		app.UseSwaggerUI();
		app.MapControllers();

		using(var scope = app.Services.CreateScope())
		{
			var dbSevice = scope.ServiceProvider.GetRequiredService<DatabaseService>();
			await dbSevice.EnsureSchema();
		}

		app.Run();
	}
}
