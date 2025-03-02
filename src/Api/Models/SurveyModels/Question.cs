using System.ComponentModel.DataAnnotations;

namespace Api.Models.SurveyModels;

/// <summary>
/// Вопрос анкеты.
/// </summary>
public class Question
{
    public int Id { get; set; }

    /// <summary>
    /// Id анкеты, к которой относится данный вопрос.
    /// </summary>
    public int SurveyId { get; set; }

    /// <summary>
    /// Заголовок вопроса.
    /// </summary>
    [Required]
    public string Title { get; set; } = null!;

    // * Подумать о том, какие поля могут быть у каждой сущности *

    // /// <summary>
    // /// Порядковый номер вопроса в анкете.
    // /// </summary>
    // public int OrderId { get; set; }
    //  
    // /// <summary>
    // /// Описание вопроса.
    // /// </summary>
    // public string? Description { get; set; }
    //
    // /// <summary>
    // /// Минимальное количество вариантов ответов, которые необходимо выбрать для ответа на вопрос.
    // /// </summary>
    // /// <remarks>
    // /// Если <see cref="MinimumAnswers"/> == 0, то вопрос возможно пропустить. Если
    // /// <see cref="MinimumAnswers"/> == 1 и <see cref="MaximumAnswers"/> == 1, то
    // /// вопрос допускает выбор одного ответа (radio button). Остальные вариации допускают диапазон
    // /// числа выбранных ответов от <see cref="MinimumAnswers"/> до <see cref="MaximumAnswers"/> (checkbox).
    // /// </remarks>
    // public int MinimumAnswers { get; set; } = 1;
    //
    // /// <inheritdoc cref="MinimumAnswers"/>
    // /// <summary>
    // /// Максимальное количество вариантов ответов, которые необходимо выбрать для ответа на вопрос.
    // /// </summary>
    // public int MaximumAnswers { get; set; } = 1;
}
