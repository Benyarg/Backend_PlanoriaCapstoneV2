//Revisado
using Backend_PlanoriaCapstone.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Quiz.Requests;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/quizzes")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        //(CRUD)

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            if (courseId.HasValue)
            {
                var items = await _quizService.GetByCourseIdAsync(courseId.Value, userId.Value);
                return Ok(items);
            }
            var all = await _quizService.GetAllAsync(userId.Value);
            return Ok(all);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _quizService.GetByIdAsync(id, userId.Value);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuizRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _quizService.CreateAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuizRequestDto request)
        {
            var result = await _quizService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _quizService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> Duplicate(int id, [FromBody] DuplicateQuizRequestDto request)
        {
            var result = await _quizService.DuplicateAsync(id, request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        //Gestión de Preguntas
        [HttpGet("{id}/questions")]

        public async Task<IActionResult> GetQuestions(int id)
        {
            var questions = await _quizService.GetQuestionsAsync(id);
            return Ok(questions);
        }

        [HttpPost("{id}/questions")]
        public async Task<IActionResult> CreateQuestion(int id, [FromBody] CreateQuestionRequestDto request)
        {
            var result = await _quizService.CreateQuestionAsync(id, request);
            return CreatedAtAction(nameof(GetQuestions), new { id }, result);
        }

        [HttpPut("{id}/questions/{questionId:int}")]
        public async Task<IActionResult> UpdateQuestion(int id, int questionId, [FromBody] UpdateQuestionRequestDto request)
        {
            var result = await _quizService.UpdateQuestionAsync(questionId, request);
            return Ok(result);
        }

        [HttpDelete("{id}/questions/{questionId:int}")]
        public async Task<IActionResult> DeleteQuestion(int id, int questionId)
        {
            var deleted = await _quizService.DeleteQuestionAsync(questionId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/questions/reorder")]
        public async Task<IActionResult> ReorderQuestions(int id, [FromBody] List<ReorderQuestionsRequestDto> request)
        {
            await _quizService.ReorderQuestionsAsync(id, request);
            return NoContent();
        }

        //Gestión de Opciones por pregunta
        [HttpPost("{id}/questions/{questionId:int}/options")]
        public async Task<IActionResult> CreateOption(int id, int questionId, [FromBody] CreateOptionRequestDto request)
        {
            var result = await _quizService.CreateOptionAsync(questionId, request);
            return CreatedAtAction(nameof(GetQuestions), new { id }, result);
        }

        [HttpPut("{id}/questions/{questionId:int}/options/{optionId:int}")]
        public async Task<IActionResult> UpdateOption(int id, int questionId, int optionId, [FromBody] UpdateOptionRequestDto request)
        {
            var result = await _quizService.UpdateOptionAsync(optionId, request);
            return Ok(result);
        }

        [HttpDelete("{id}/questions/{questionId:int}/options/{optionId:int}")]
        public async Task<IActionResult> DeleteOption(int id, int questionId, int optionId)
        {
            var deleted = await _quizService.DeleteOptionAsync(optionId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        //Configuración y Ajustes (Settings) de quiz
        [HttpGet("{id}/settings")]
        public async Task<IActionResult> GetSettings(int id)
        {
            var settings = await _quizService.GetSettingsAsync(id);
            return Ok(settings);
        }

        [HttpPut("{id}/settings")]
        public async Task<IActionResult> UpdateSettings(int id, [FromBody] UpdateQuizSettingsRequestDto settings)
        {
            await _quizService.UpdateSettingsAsync(id, settings);
            return NoContent();
        }

        [HttpPost("{id}/settings/reset")]
        public async Task<IActionResult> ResetSettings(int id)
        {
            await _quizService.ResetSettingsAsync(id);
            return NoContent();
        }

        //Vista Previa y Simulación
        [HttpGet("{id}/preview")]
        public async Task<IActionResult> Preview(int id)
        {
            var preview = await _quizService.PreviewAsync(id);
            return Ok(preview);
        }

        [HttpPost("{id}/simulate")]
        public async Task<IActionResult> Simulate(int id)
        {
            var simulation = await _quizService.SimulateAsync(id);
            return Ok(simulation);
        }
    }
}
