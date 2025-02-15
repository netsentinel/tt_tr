using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public sealed class Message
{
	public long Id { get; set; }

	[Required]
	public long Seq { get; set; }

	public DateTime CreatedAt { get; set; }

	[Required]
	[MinLength(1)]
	[MaxLength(128)]
	public string Text { get; set; }
}
