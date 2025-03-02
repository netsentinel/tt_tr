using System.ComponentModel.DataAnnotations;

namespace Api.Models.SurveyModels;

/// <summary>
/// Вариант ответа на вопрос анкеты.
/// </summary>
public class Answer
{
    public int Id { get; set; }

    /// <summary>
    /// Id вопроса, к которому относится данный вариант ответа.
    /// </summary>
    public int QuestionId { get; set; }

    /// <summary>
    /// Заголовок варианта ответа.
    /// </summary>
    [Required]
    public string Title { get; set; } = null!;

    // * Подумать о том, какие поля могут быть у каждой сущности *

    // /// <summary>
    // /// Порядковый номер варианта ответа.
    // /// </summary>
    // public int OrderId { get; set; }
    //
    // /// <summary>
    // /// Описание варианта ответа.
    // /// </summary>
    // public string? Description { get; set; }
    //
    // public bool AllowsFreeTextInput { get; set; }
    // public string FreeTextInputRegexEcma262 { get; set; }
}