using Api.Models.SurveyModels;

namespace Api.Models.RequestResponseModels;

public class QuestionWithAnswersResponse
{
    public required Question Question { get; set; }
    public required List<Answer> Answers { get; set; }
}
