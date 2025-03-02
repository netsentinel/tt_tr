using Api.Models.InterviewModels;

namespace Api.Models.SurveyModels;

public class ResultAnswer : InterviewAnswer
{
    // Вообще не увидел смысла в этом классе, я бы сделал пропс Completed в Interview,
    // разве что таблица Interviews будет на SSD, а Results на HDD, например.
}