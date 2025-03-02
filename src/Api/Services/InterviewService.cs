using Api.Models.InterviewModels;
using Api.Models.SurveyModels;
using Dapper;
using Npgsql;
using static Api.Services.RepositoryHelper;

namespace Api.Services;

public class InterviewService(NpgsqlConnection _sqlConnection)
{
	/// <returns>Id созданного в базе данных интервью.</returns>
	public async Task<int> CreateInterview(int surveyId)
	{
		var sql = GenerateInsertQuery<Interview>(true);
		return await _sqlConnection.QuerySingleAsync<int>(sql, new Interview
		{
			SurveyId = surveyId,
		});
	}

    /// <returns>Id первого вопроса для данного интервью.</returns>
    public async Task<int> GetFirstQuestionId(int interviewId)
	{
		var sql =
			$"""
				WITH searchSurveyId AS (
					SELECT {nameof(Interview.SurveyId)} FROM {nameof(Interview)}s WHERE 
					{nameof(Interview.Id)}={interviewId} LIMIT 1
				)

				SELECT {nameof(Question.Id)} FROM {nameof(Question)}s WHERE
					{nameof(Question.SurveyId)} IN (SELECT * FROM searchSurveyId)
				ORDER BY {nameof(Question.Id)} ASC LIMIT 1
			""";

		return await _sqlConnection.QueryFirstAsync<int>(sql);
	}

    /// <summary>
	/// Сохраняет ответ на один вопрос в некотором интервью в базу данных.
	/// </summary>
    public async Task SaveAnswers(InterviewAnswer answer)
	{
		/* Если прилетает одновременно два запроса на вставку ответа на один вопрос с единственным
		 * вариантом ответа, оба быстро проходят DELETE FROM и заходят в INSERT - создаётся два
		 * ответа на вопрос, подразумевавший только один ответ. Поэтому SERIALIZABLE.
		 */
		var sql = $"""
				BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE;                  
				
				DELETE FROM {nameof(InterviewAnswer)}s WHERE
					{nameof(InterviewAnswer.InterviewId)}={answer.InterviewId} AND
					{nameof(InterviewAnswer.QuestionId)}={answer.QuestionId};
				
				INSERT INTO {nameof(InterviewAnswer)}s (
					{nameof(InterviewAnswer.InterviewId)}, 
					{nameof(InterviewAnswer.QuestionId)}, 
					{nameof(InterviewAnswer.AnswerId)}) 
				VALUES ({answer.InterviewId}, {answer.QuestionId}, {answer.AnswerId});
				
				COMMIT;
			""";

		await _sqlConnection.ExecuteAsync(sql);
	}

	/// <returns>Id следующего после lastAnswer вопроса или 0, если вопрос был последним.</returns>
	public async Task<int> GetNextQuestionId(InterviewAnswer lastAnswer)
	{
		var sql = 
			$"""
				WITH SurveyId AS (
					SELECT {nameof(Question.SurveyId)} FROM {nameof(Question)}s WHERE 
					{nameof(Question.Id)}={lastAnswer.QuestionId} LIMIT 1
				)

				SELECT {nameof(Question.Id)} FROM {nameof(Question)}s WHERE
					{nameof(Question.Id)} > {lastAnswer.QuestionId} AND
					{nameof(Question.SurveyId)}=SurveyId
				ORDER BY {nameof(Question.Id)} ASC LIMIT 1
			""";

		return (await _sqlConnection.QueryAsync<int>(sql)).FirstOrDefault();
	}

	/// <summary>
	/// Завершает интервью, перемещая его в таблицу результатов.
	/// </summary>
	public async Task CompleteInterview(int interviewId)
	{
        // SERIALIZABLE т.к. тут может произойти вставка до удаления из таблицы Interviews,
		// но в принципе конкретно в данном случае можно и забить, согласованность не нарушится
        var sql = $"""
				BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE;
				
				CREATE TEMP TABLE temp_interview_answers AS 
				SELECT * FROM {nameof(InterviewAnswer)}s WHERE {nameof(InterviewAnswer.InterviewId)}={interviewId};
				
				CREATE TEMP TABLE temp_interviews AS 
				SELECT * FROM {nameof(Interview)}s WHERE {nameof(Interview.Id)}={interviewId};
				
				DELETE FROM {nameof(InterviewAnswer)}s 
				WHERE {nameof(InterviewAnswer.InterviewId)}={interviewId};
				
				DELETE FROM {nameof(Interview)}s 
				WHERE {nameof(Interview.Id)}={interviewId};
				
				INSERT INTO {nameof(Result)}s
				SELECT * FROM temp_interviews;
				
				INSERT INTO {nameof(ResultAnswer)}s
				SELECT * FROM temp_interview_answers;
				
				COMMIT; 
			""";

		await _sqlConnection.ExecuteAsync(sql);
	}
}