using Api.Models.InterviewModels;
using Api.Models.SurveyModels;
using Dapper;
using Npgsql;

namespace Api.Repositories;

public class DatabaseService(
    NpgsqlConnection _sqlConnection, ILogger<DatabaseService>? _logger)
{
	/// <summary>
	/// Создаёт схему базы данных.
	/// </summary>
    public async Task EnsureSchema()
    {
        var sql = $"""
			BEGIN TRANSACTION;

			CREATE TABLE {nameof(Survey)}s(
				{nameof(Survey.Id)} SERIAL PRIMARY KEY,
				{nameof(Survey.Title)} TEXT NOT NULL
			);
			
			CREATE TABLE {nameof(Question)}s(
				{nameof(Question.Id)} SERIAL PRIMARY KEY,
				{nameof(Question.SurveyId)} INTEGER NOT NULL REFERENCES {nameof(Survey)}s({nameof(Survey.Id)}) ON DELETE RESTRICT,
				{nameof(Question.Title)} TEXT NOT NULL
			);
			
			CREATE TABLE {nameof(Answer)}s(
				{nameof(Answer.Id)} SERIAL PRIMARY KEY,
				{nameof(Answer.QuestionId)} INTEGER NOT NULL REFERENCES {nameof(Question)}s({nameof(Question.Id)}) ON DELETE RESTRICT,
				{nameof(Answer.Title)} TEXT NOT NULL
			);
			
			CREATE TABLE {nameof(Interview)}s(
				{nameof(Interview.Id)} SERIAL PRIMARY KEY,
				{nameof(Interview.SurveyId)} INTEGER NOT NULL REFERENCES {nameof(Survey)}s({nameof(Survey.Id)}) ON DELETE RESTRICT
			);
			
			CREATE TABLE {nameof(InterviewAnswer)}s(
				{nameof(InterviewAnswer.Id)} SERIAL PRIMARY KEY,
				{nameof(InterviewAnswer.InterviewId)} INTEGER NOT NULL REFERENCES {nameof(Interview)}s({nameof(Interview.Id)}) ON DELETE RESTRICT,
				{nameof(InterviewAnswer.QuestionId)} INTEGER NOT NULL REFERENCES {nameof(Question)}s({nameof(Question.Id)}) ON DELETE RESTRICT,
				{nameof(InterviewAnswer.AnswerId)} INTEGER NOT NULL REFERENCES {nameof(Answer)}s({nameof(Answer.Id)}) ON DELETE RESTRICT,
				CONSTRAINT {nameof(InterviewAnswer)}_unique_interview_question_answer UNIQUE 
					({nameof(InterviewAnswer.InterviewId)}, {nameof(InterviewAnswer.QuestionId)}, {nameof(InterviewAnswer.AnswerId)})
			);

			CREATE INDEX {nameof(InterviewAnswer)}_interviewanswer_interview_question_answer_idx ON {nameof(InterviewAnswer)}s 
			USING BTREE ({nameof(InterviewAnswer.InterviewId)}, {nameof(InterviewAnswer.QuestionId)}, {nameof(InterviewAnswer.AnswerId)});

			-- really needed?
	
			CREATE TABLE {nameof(Result)}s(
				{nameof(Result.Id)} INT PRIMARY KEY,
				{nameof(Result.SurveyId)} INTEGER NOT NULL REFERENCES {nameof(Survey)}s({nameof(Survey.Id)}) ON DELETE RESTRICT
			);
			
			CREATE TABLE {nameof(ResultAnswer)}s(
				{nameof(ResultAnswer.Id)} INT PRIMARY KEY,
				{nameof(ResultAnswer.InterviewId)} INTEGER NOT NULL REFERENCES {nameof(Result)}s({nameof(Interview.Id)}) ON DELETE RESTRICT,
				{nameof(ResultAnswer.QuestionId)} INTEGER NOT NULL REFERENCES {nameof(Question)}s({nameof(Question.Id)}) ON DELETE RESTRICT,
				{nameof(ResultAnswer.AnswerId)} INTEGER NOT NULL REFERENCES {nameof(Answer)}s({nameof(Answer.Id)}) ON DELETE RESTRICT,
				CONSTRAINT {nameof(ResultAnswer)}_unique_interview_question_answer UNIQUE 
					({nameof(ResultAnswer.InterviewId)}, {nameof(ResultAnswer.QuestionId)}, {nameof(ResultAnswer.AnswerId)})
			);

			CREATE INDEX {nameof(ResultAnswer)}_interviewanswer_interview_question_answer_idx ON {nameof(ResultAnswer)}s 
			USING BTREE ({nameof(ResultAnswer.InterviewId)}, {nameof(ResultAnswer.QuestionId)}, {nameof(ResultAnswer.AnswerId)});

			COMMIT;
		""";

        try
        {
            await _sqlConnection.ExecuteAsync(sql);
            _logger?.LogInformation("Database schema ensured.");
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            _logger?.LogInformation("Database schema already exists.");
        }
    }
}