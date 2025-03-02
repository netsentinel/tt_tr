using Api.Models.RequestResponseModels;
using Api.Models.SurveyModels;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionController(
    BaseRepository _baseRepo, SurveyService _surveyService) : ControllerBase
{
    /// <summary>
    /// Возвращает вопрос с ответами согласно переданному Id.
    /// </summary>
    [HttpGet]
    [Route("{Id:int}/with-answers")]
    public async Task<IActionResult> GetQuestion([FromRoute][Required] int Id)
    {
        var response = new QuestionWithAnswersResponse()
        {
            Question = (await _baseRepo.Get<Question>(Id)).Single(),
            Answers = (await _surveyService.GetAnswersForQuestion(Id)).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Возвращает все вопросы из базы данных.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetQuestion()
        => Ok(await _baseRepo.Get<Question>());

    /// <summary>
    /// Создаёт вопрос и возвращает его Id в базе данных.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody][Required] Question request)
        => Ok(await _baseRepo.Create(request));
}