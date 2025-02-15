using Api.DataAccess;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Data.Common;
using System.Text.Json;

namespace Api;

public class Program
{
	public static void Main(string[] args)
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

		builder.Services.AddScoped<MessageRepository>();
		builder.Services.AddSingleton<MessageRealtimeService>();

		var app = builder.Build();

		app.UseWebSockets();

		app.UseSwagger();
		app.UseSwaggerUI();
		app.MapControllers();

		using(var scope = app.Services.CreateScope())
		{
			var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

			using var conn = scope.ServiceProvider.GetRequiredService<NpgsqlConnection>();
			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"""
				CREATE TABLE IF NOT EXISTS {nameof(Message)}s (
					{nameof(Message.Id)} SERIAL8 PRIMARY KEY,
					{nameof(Message.Seq)} INT8 NOT NULL,
					{nameof(Message.CreatedAt)} TIMESTAMP NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
					{nameof(Message.Text)} VARCHAR(128) NOT NULL
				);
			""";

			try
			{
				conn.Open();
				cmd.ExecuteNonQuery();
				logger.LogInformation("Database schema ensured.");
			}
			catch(Exception ex)
			{
				logger.LogCritical($"Failed to create database schema: '{ex.Message}'.");
				throw;
			}
		}

		app.Run();
	}
}
