namespace Api.Models.InterviewModels;

/// <summary>
/// Ответ на вопрос в интервью.
/// </summary>
public class InterviewAnswer
{
    public int Id { get; set; }

    /// <summary>
    /// Id интервью, к которому относится данный ответ.
    /// </summary>
    public int InterviewId { get; set; }
    
    /// <summary>
    /// Id вопроса, к которому относится данный ответ.
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// Id ответа, выбранного пользователем.
    /// </summary>
    public int AnswerId { get; set; }
}