using Api.Models.SurveyModels;
using Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnswerController(BaseRepository _baseRepo) : ControllerBase
{
    /// <summary>
    /// Возвращает все ответы из базы данных.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAnswers()
        => Ok(await _baseRepo.Get<Answer>());

    /// <summary>
    /// Создаёт ответ и возвращает его Id в базе данных.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAnswer([FromBody] Answer request)
        => Ok(await _baseRepo.Create(request));
}