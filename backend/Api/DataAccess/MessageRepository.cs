using Api.Models;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.Json;

namespace Api.DataAccess;

public sealed class MessageRepository(
	NpgsqlConnection sqlConnection, ILogger<MessageRepository> logger)
{
	private readonly NpgsqlConnection _sqlConnection = sqlConnection;
	private readonly ILogger<MessageRepository> _logger = logger;

	public async Task AddMessage(Message msg)
	{
		var sql = $"""
			INSERT INTO {nameof(Message)}s
				({nameof(Message.Seq)}, {nameof(Message.CreatedAt)}, {nameof(Message.Text)})
			VALUES
				(@{nameof(Message.Seq)}, @{nameof(Message.CreatedAt)}, @{nameof(Message.Text)})
			RETURNING {nameof(Message.Id)}
		""";

		using var command = new NpgsqlCommand(sql, _sqlConnection);
		command.Parameters.AddWithValue($"@{nameof(Message.Seq)}", msg.Seq);
		command.Parameters.AddWithValue($"@{nameof(Message.CreatedAt)}", msg.CreatedAt);
		command.Parameters.AddWithValue($"@{nameof(Message.Text)}", msg.Text);

		await _sqlConnection.OpenAsync();
		var response = await command.ExecuteScalarAsync();
		msg.Id = Convert.ToInt32(response);

		_logger.LogInformation(
			$"{nameof(AddMessage)}: added new message with " +
			$"{nameof(msg.Id)}='{msg.Id}', " +
			$"{nameof(msg.Seq)}='{msg.Seq}', " +
			$"{nameof(msg.CreatedAt)}='{msg.CreatedAt.ToString("s")}', " +
			$"len({nameof(msg.Text)})='{msg.Text.Length}'.");
	}

	public async Task<List<Message>> GetMessages(DateTime from, DateTime to)
	{
		var sql = $"""
			SELECT 
				{nameof(Message.Id)}, {nameof(Message.Seq)}, 
				{nameof(Message.CreatedAt)}, {nameof(Message.Text)}
			FROM 
				{nameof(Message)}s
			WHERE
				{nameof(Message.CreatedAt)} BETWEEN @{nameof(from)} AND @{nameof(to)}
		""";

		using var command = new NpgsqlCommand(sql, _sqlConnection);
		command.Parameters.AddWithValue($"@{nameof(from)}", from);
		command.Parameters.AddWithValue($"@{nameof(to)}", to);

		await _sqlConnection.OpenAsync();
		using var reader = await command.ExecuteReaderAsync();

		var records = new List<Message>();
		while(await reader.ReadAsync())
		{
			records.Add(new Message
			{
				Id = reader.GetInt32(0),
				Seq = reader.GetInt64(1),
				CreatedAt = reader.GetDateTime(2),
				Text = reader.GetString(3),
			});
		}

		_logger.LogInformation($"{nameof(GetMessages)}: queried '{records.Count}' messages between '{from.ToString("s")}' and '{to.ToString("s")}', ids: '{string.Join(", ", records.Select(x => x.Id))}'.");

		return records;
	}
}