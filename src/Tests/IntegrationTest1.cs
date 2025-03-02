using Api.Controllers;
using Api.Models.RequestResponseModels;
using Api.Models.SurveyModels;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace Tests;

public class IntegrationTest1(ITestOutputHelper _output)
{
    [Fact]
    public async Task Test1()
    {
        var pgsqlContainer = new PostgreSqlBuilder().Build();
        try
        {
            await pgsqlContainer.StartAsync();
            await Test1Internal(pgsqlContainer.GetConnectionString());
        }
        finally
        {
            await pgsqlContainer.DisposeAsync();
        }
    }

    private async Task Test1Internal(string postgresConnectionString)
    {
        var sqlConnection = new NpgsqlConnection(postgresConnectionString);
        sqlConnection.Open();

        var dbService = new DatabaseService(sqlConnection, null);
        var baseRepo = new BaseRepository(sqlConnection);
        var surveyService = new SurveyService(sqlConnection);
        var interviewService = new InterviewService(sqlConnection);

        var questionController = new QuestionController(baseRepo, surveyService);
        var answerController = new AnswerController(baseRepo);
        var surveyConrtoller = new SurveyController(baseRepo, surveyService);
        var interviewController = new InterviewController(baseRepo, interviewService);

        // создаём схему

        await dbService.EnsureSchema();

        // создаём анкету

        var surveyId = (int)(await surveyConrtoller.CreateSurvey(new()
        {
            Title = "test survey"
        }) as ObjectResult)!.Value!;

        Assert.NotEqual(0, surveyId);

        // создаем два вопроса

        var question1Id = (int)(await questionController.CreateQuestion(new()
        {
            Title = "test question 1",
            SurveyId = surveyId
        }) as ObjectResult)!.Value!;

        var question2Id = (int)(await questionController.CreateQuestion(new()
        {
            Title = "test question 2",
            SurveyId = surveyId
        }) as ObjectResult)!.Value!;

        Assert.NotEqual(0, question1Id);
        Assert.NotEqual(0, question2Id);
        Assert.NotEqual(question1Id, question2Id);

        // создаем 2 варианта для первого и 3 варианта для второго вопросов

        var answer11Id = (int)(await answerController.CreateAnswer(new()
        {
            Title = "test answer 1 for question 1",
            QuestionId = question1Id
        }) as ObjectResult)!.Value!;

        var answer12Id = (int)(await answerController.CreateAnswer(new()
        {
            Title = "test answer 2 for question 1",
            QuestionId = question1Id
        }) as ObjectResult)!.Value!;

        var answer21Id = (int)(await answerController.CreateAnswer(new()
        {
            Title = "test answer 1 for question 2",
            QuestionId = question2Id
        }) as ObjectResult)!.Value!;

        var answer22Id = (int)(await answerController.CreateAnswer(new()
        {
            Title = "test answer 2 for question 2",
            QuestionId = question2Id
        }) as ObjectResult)!.Value!;

        var answer23Id = (int)(await answerController.CreateAnswer(new()
        {
            Title = "test answer 3 for question 2",
            QuestionId = question2Id
        }) as ObjectResult)!.Value!;

        Assert.NotEqual(0, answer11Id);
        Assert.NotEqual(0, answer12Id);
        Assert.NotEqual(0, answer21Id);
        Assert.NotEqual(0, answer22Id);
        Assert.NotEqual(0, answer23Id);
        Assert.NotEqual(answer11Id, answer12Id);
        Assert.NotEqual(answer21Id, answer22Id);
        Assert.NotEqual(answer21Id, answer23Id);
        Assert.NotEqual(answer22Id, answer23Id);

        // создаем сессию интервью

        var interviewId = (int)(await interviewController.StartInterview(surveyId) as ObjectResult)!.Value!;

        Assert.NotEqual(0, interviewId);

        // получаем id первого вопроса

        var firstQuestionId = (int)(await interviewController.GetFirstQuestion(interviewId) as ObjectResult)!.Value!;

        Assert.Equal(question1Id, firstQuestionId);

        // получаем первый вопрос и опции

        var optionsForQuestion1 = (QuestionWithAnswersResponse)(await questionController.GetQuestion(firstQuestionId) as ObjectResult)!.Value!;

        Assert.Equal(question1Id, optionsForQuestion1.Question.Id);
        Assert.Equal(2, optionsForQuestion1.Answers.Count);

        // отвечаем на первый вопрос и получаем id второго вопроса

        var questionAfterSaving1 = (int)(await interviewController.SaveAnswer(new()
        {
            QuestionId = question1Id,
            InterviewId = interviewId,
            AnswerId = optionsForQuestion1.Answers[0].Id,
        }) as ObjectResult)!.Value!;

        Assert.Equal(question2Id, questionAfterSaving1);

        // аналогично для второго вопроса

        var optionsForQuestion2 = (QuestionWithAnswersResponse)(await questionController.GetQuestion(question2Id) as ObjectResult)!.Value!;

        Assert.Equal(question2Id, optionsForQuestion2.Question.Id);
        Assert.Equal(3, optionsForQuestion2.Answers.Count);

        var questionAfterSaving2 = (int)(await interviewController.SaveAnswer(new()
        {
            QuestionId = question2Id,
            InterviewId = interviewId,
            AnswerId = optionsForQuestion2.Answers[2].Id,
        }) as ObjectResult)!.Value!;

        Assert.Equal(0, questionAfterSaving2);

        // завершаем интервьюи и сохраняемся в таблицу Results

        await interviewController.CompleteInterview(interviewId);

        // более нельзя записывать ответы

        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await interviewController.SaveAnswer(new()
            {
                QuestionId = question2Id,
                InterviewId = interviewId,
                AnswerId = optionsForQuestion2.Answers[0].Id,
            });
        });

        // коннект выбило ошибкой в транзакции
        sqlConnection.Close();
        sqlConnection.Open();

        // проверяем наличие ответов в БД

        var results = await baseRepo.Get<Result>();
        var found = results.Where(x => x.Id == interviewId && x.SurveyId == surveyId).Single();
        var resultsAnswers = await baseRepo.Get<ResultAnswer>();

        var relatedAnswers = resultsAnswers.Where(x => x.InterviewId == found.Id)
            .OrderBy(x => x.QuestionId).ToList();

        Assert.Equal(2, relatedAnswers.Count);
        Assert.Equal(answer11Id, relatedAnswers[0].AnswerId);
        Assert.Equal(answer23Id, relatedAnswers[1].AnswerId);

        _output.WriteLine(string.Join('\n', [
            $"Ответы в базе данны для интервью {found.Id} анкеты {found.SurveyId}:",
            ..resultsAnswers.Select(x=> $"Вопрос {x.QuestionId} -> ответ {x.AnswerId}")
        ]));
    }
}
