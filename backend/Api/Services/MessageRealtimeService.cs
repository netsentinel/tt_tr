using Api.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Api.Services;


public sealed class MessageRealtimeService(ILogger<MessageRealtimeService> logger)
{
	private readonly ILogger<MessageRealtimeService> _logger = logger;
	private readonly List<Message> _messages = new();

	private int _version = 0;
	private DateTime _lastVersionChange = DateTime.MinValue;

	public event Action? MessageAdded;
	public long LastMessageId { get; private set; }

	public void AddMessage(Message msg)
	{
		/* Можно сделать lock-free. Например, создать поле _version,
		 * которое через Interlocked инкрементировать при добавлении
		 * каждого нового сообщения. Сам _messages сделать ConcurrentBag. 
		 * В методе WaitForUpdates проверять, что до и после фильтрации 
		 * сообщений через Where версии совпали. Однако такой код начнет 
		 * захлебываться на большом числе апдейтов. Это тоже можно пофиксить, 
		 * введя сюда условие, вида
		 *		if(DateTime.UtcNow.Second % 2 == 0)
		 *			await Task.Delay(500);
		 * т.е. ограничив запись до одного раза в секунду. В нечетные секунды
		 * мы записываем, в четные читаем. Но тогда получаем здесь падение 
		 * производительности на ровном месте. Судя по всему, лучшим решением 
		 * будет действительно применить самую обычную блокировку. В данном
		 * случае _version остается исключительно на тот случай, если при чтении
		 * мы попадем на очистку. Можно ввести двойную буферизацию, чтобы лог
		 * не брался чаще, чем раз в секунду. Сначала сообщения прибывают в очередь,
		 * потом раз в секунду сбрасываются через лок.
		 */
		lock(_messages)
		{
			_messages.Add(msg);
			LastMessageId = msg.Id;

			_logger.LogInformation($"{nameof(AddMessage)}: added new message with {nameof(msg.Id)}='{msg.Id}', {nameof(msg.Seq)}='{msg.Seq}'.");

			/* 1024 и 1 взяты на обум.
			 * Проверка по времени нужна, чтобы на большом потоке не захлебнулся. На большом потоке
			 * возможна копеечная оптимизация путем динамического изменения числа 1024, если последнее 
			 * накопление 1024 сообщений заняло до 30 секунд, то уменьшаем в 2 раза, если больше 90 секунд, то 
			 * увеличиваем в два раза... прямо как механизм сложности хеша в биткоине от времени создания
			 * последнего блока. Эта оптимизация устранит вызов DateTime.UtcNow, который бьёт ОС, каждый раз, 
			 * когда превышено статическое 1024, но ещё не истекло время.
			 */
			if(_messages.Count > 1024 && (DateTime.UtcNow - _lastVersionChange).TotalMinutes > 1)
			{
				var removeCount = _messages.Count / 8 * 7;
				_messages.RemoveRange(0, removeCount);
				_lastVersionChange = DateTime.UtcNow;
				_version++;
				_logger.LogInformation($"{nameof(AddMessage)}: messages cache was cleaned, removed '{removeCount}' messages, new version is '{_version}'.");
			}
		}
		MessageAdded?.Invoke();
	}

	public List<Message> GetUpdates(long sinceId)
	{
		// нет сообщений
		if(_messages.Count == 0)
		{
			_logger.LogInformation($"{nameof(GetUpdates)}: no messages ({nameof(_messages)} is empty).");
			return [];
		}

		// клиенту уже известно о последнем сообщении
		if(_messages[^1].Id <= sinceId)
		{
			_logger.LogInformation($"{nameof(GetUpdates)}: no new messages since id='{sinceId}'.");
			return [];
		}

		var searchDummy = new Message { Id = sinceId };
		while(true)
		{
			// оптимистичная блокировка по версии
			var version = _version;

			// ищем сообщение, с которого начинаются новые
			var newMessagesStartIndex = _messages.BinarySearch(searchDummy,
				Comparer<Message>.Create((x, y) => x.Id.CompareTo(y.Id)));

			if(newMessagesStartIndex >= 0) newMessagesStartIndex++;
			else newMessagesStartIndex = -(newMessagesStartIndex + 1);

			// слайс новых сообщений в лист
			List<Message> result = null!;
			try
			{
				result = _messages.Slice(newMessagesStartIndex, _messages.Count - newMessagesStartIndex);
			}
			catch(ArgumentException ex)
			{
				// ошибка произошла не из-за параллельной очистки, а по какой-то неизвестной причине
				if(version == _version) throw;
			}

			// count нужно получить пока мы в блоке с защитой от изменений версии
			var count = _messages.Count;
			if(version != _version)
			{
				_logger.LogInformation($"{nameof(GetUpdates)}: version has changed from '{version}' to '{_version}' since the request. Querying again...");
				continue;
			}

			_logger.LogInformation($"{nameof(GetUpdates)}: retrieved messages from the realtime cache: " +
				$"count='{count}', skip='{newMessagesStartIndex}', take='{result.Count}', version='{version}'.");

			return result;
		}
	}
}