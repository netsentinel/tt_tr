using Api.Models.InterviewModels;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

public class InterviewController(BaseRepository _baseRepo, InterviewService _interviewService) : ControllerBase
{
    /// <summary>
    /// Начинает новое интервью и возвращает его Id.
    /// </summary>
    [HttpPost]
    [Route("start")]
    public async Task<IActionResult> StartInterview([FromQuery][Required] int surveyId)
        => Ok(await _interviewService.CreateInterview(surveyId));

    /// <summary>
    /// Возвращает Id первого вопроса для данного интервью.
    /// </summary>
    [HttpGet]
    [Route("{interviewId:int}/first-question")]
    public async Task<IActionResult> GetFirstQuestion([FromRoute][Required] int interviewId) 
        => Ok(await _interviewService.GetFirstQuestionId(interviewId));

    /// <summary>
    /// Отвечает на вопрос интервью и возвращает Id следующего вопроса.
    /// </summary>
    [HttpPost]
    [Route("answer")]
    // можно сделать {interviewId:int}, но тогда нужно писать DTO с омитом соответствующего поля из модели
    public async Task<IActionResult> SaveAnswer([FromBody][Required] InterviewAnswer answer)
    {
        await _interviewService.SaveAnswers(answer);
        return Ok(await _interviewService.GetNextQuestionId(answer));
    }

    /// <summary>
    /// Завершает интервью и сохраняет результат.
    /// </summary>
    [HttpPost]
    [Route("{surveyId:int}/complete")]
    public async Task<IActionResult> CompleteInterview(
        [FromRoute][Required] int interviewId)
    {
        await _interviewService.CompleteInterview(interviewId);
        return Ok();
    }
}