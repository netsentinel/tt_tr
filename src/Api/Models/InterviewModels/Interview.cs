namespace Api.Models.InterviewModels;

/// <summary>
/// Информация об интервью (отдельной сессии прохождения анкеты).
/// </summary>
public class Interview
{
    public int Id { get; set; }

    /// <summary>
    /// Id анкеты, к которому относится данная сессия.
    /// </summary>
    public int SurveyId { get; set; }

    // * Подумать о том, какие поля могут быть у каждой сущности *

    // /// <summary>
    // /// Дата начала интервью.
    // /// </summary>
    // public DateTime CreatedAt { get; set; }
    //
    // /// <summary>
    // /// Дата последнего обновления интервью.
    // /// </summary>
    // public DateTime UpdatedAt { get; set; }
}