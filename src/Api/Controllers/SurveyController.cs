using Api.Models.SurveyModels;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveyController(BaseRepository _baseRepo, SurveyService _surveyService) : ControllerBase
{
    /// <summary>
    /// Возвращает все анкеты из базы данных.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSurveys()
        => Ok(await _baseRepo.Get<Survey>());

    /// <summary>
    /// Создаёт анкету и возвращает её Id в базе данных.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSurvey([FromBody] Survey request)
        => Ok(await _baseRepo.Create(request));

    /// <summary>
    /// Автоматически создаёт тестовую анкету и возвращает её Id в базе данных.
    /// </summary>
    [HttpPost]
    [Route("seed")]
    public async Task<IActionResult> Seed()
    {
        var dateId = DateTime.UtcNow.ToString("s");

        var surveyId = await _baseRepo.Create(new Survey
        {
            Title = $"Тестовая анкета {dateId}"
        });

        var question1Id = await _baseRepo.Create(new Question
        {
            SurveyId = surveyId,
            Title = $"Тестовый вопрос 1 анкеты {surveyId}"
        });

        var question2Id = await _baseRepo.Create(new Question
        {
            SurveyId = surveyId,
            Title = $"Тестовый вопрос 2 анкеты {surveyId}"
        });

        var answer11Id = _baseRepo.Create(new Answer
        {
            QuestionId = question1Id,
            Title = $"Тестовый ответ 1 вопроса 1 анкеты {surveyId}"
        });

        var answer12Id = _baseRepo.Create(new Answer
        {
            QuestionId = question1Id,
            Title = $"Тестовый ответ 2 вопроса 1 анкеты {surveyId}"
        });

        var answer21Id = _baseRepo.Create(new Answer
        {
            QuestionId = question2Id,
            Title = $"Тестовый ответ 1 вопроса 2 анкеты {surveyId}"
        });

        var answer22Id = _baseRepo.Create(new Answer
        {
            QuestionId = question2Id,
            Title = $"Тестовый ответ 2 вопроса 2 анкеты {surveyId}"
        });

        var answer23Id = _baseRepo.Create(new Answer
        {
            QuestionId = question2Id,
            Title = $"Тестовый ответ 3 вопроса 2 анкеты {surveyId}"
        });

        return Ok(surveyId);
    }
}