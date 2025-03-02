using Api.Models.SurveyModels;
using Dapper;
using Npgsql;

namespace Api.Services;

public class SurveyService(NpgsqlConnection _sqlConnection)
{
    /// <returns>Варианты ответа для вопроса <paramref name="questionId"/>.</returns>
    public async Task<IEnumerable<Answer>> GetAnswersForQuestion(int questionId)
    {
        var sql = $"SELECT * FROM {nameof(Answer)}s WHERE {nameof(Answer.QuestionId)}={questionId}";
        return await _sqlConnection.QueryAsync<Answer>(sql);
    }
}