using System.ComponentModel.DataAnnotations;

namespace Api.Models.SurveyModels;

/// <summary>
/// Информация об анкете.
/// </summary>
public class Survey
{
	public int Id { get; set; }

    /// <summary>
    /// Название анкеты.
    /// </summary>
    [Required]
	public string Title { get; set; } = null!;

    // * Подумать о том, какие поля могут быть у каждой сущности *

    // /// <summary>
    // /// Описание анкеты.
    // /// </summary>
    // public string? Description { get; set; }
    //
    /// <summary>
    // /// Дата создания анкеты.
    // /// </summary>
    // public DateTime CreatedAt { get; set; }
    //
    // /// <summary>
    // /// Дата начала действия анкеты (дата с которой возможно прохождение анкеты).
    // /// </summary>
    // public DateTime StartsAt { get; set; }
    //
    // /// <summary>
    // /// Дата окончания действия анкеты (дата до которой возможно прохождение анкеты).
    // /// </summary>
    // public DateTime EndsAt { get; set; }
    // 	
    // /// <summary>
    // /// Максимальное количество прохождений данной анкеты.
    // /// </summary>
    // public int MaximumInterviews { get; set; }
}
