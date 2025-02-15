using Api.DataAccess;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MessageController(
	MessageRepository repo, MessageRealtimeService service, ILogger<MessageController> logger) : ControllerBase
{
	private readonly MessageRepository _repo = repo;
	private readonly MessageRealtimeService _service = service;
	private readonly ILogger<MessageController> _logger = logger;

	[HttpGet]
	public async Task<IActionResult> GetMessages([FromQuery][Required] DateTime from, [FromQuery][Required] DateTime to)
	{
		return Ok(await _repo.GetMessages(from, to));
	}

	[HttpPost]
	public async Task<IActionResult> AddMessage([FromBody][Required] Message msg)
	{
		msg.CreatedAt = DateTime.UtcNow;
		await _repo.AddMessage(msg);
		_service.AddMessage(msg);
		return Ok(msg.Id);
	}

	[Route("ws")]
	[ApiExplorerSettings(IgnoreApi = true)]
	public async Task WebSocketMessagesStream([FromQuery][Required] long lastIdOnClient)
	{
		if(!HttpContext.WebSockets.IsWebSocketRequest)
		{
			HttpContext.Response.StatusCode = 400;
			return;
		}

		if(lastIdOnClient == 0)
			lastIdOnClient = _service.LastMessageId;

		using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
		while(webSocket.State == WebSocketState.Open)
		{
			/* Не вижу надежного способа встать в это ожидание без
			 * выполнения как минимум одного апдейта.
			 * Только сначала создать ожидание, потом выполнить обновление,
			 * потом встать в созданное ожидание. Если в момент непосредственно
			 * после заполнения updates прилетит новое сообщение, оно завершит
			 * ожидание, созданное ранее. Если же создавать ожидание уже после
			 * заполнения updates, то имеется шанс пропустить сообщение.
			 */
			var tcs = new TaskCompletionSource<bool>();
			var cancelFunc = () => tcs.SetResult(true);
			try
			{
				_service.MessageAdded += cancelFunc;
				var updates = _service.GetUpdates(lastIdOnClient);
				if(updates.Count > 0)
				{
					var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
						updates, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
					await webSocket.SendAsync(responseBytes,
						WebSocketMessageType.Text, true, CancellationToken.None);
					lastIdOnClient = updates[^1].Id;
				}
				await tcs.Task;
			}
			catch
			{
				throw;
			}
			finally
			{
				_service.MessageAdded -= cancelFunc;
			}
		}
	}
}