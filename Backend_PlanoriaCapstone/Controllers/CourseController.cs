//Revisado
using Backend_PlanoriaCapstone.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Courses.Requests;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/courses")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // Obtiene la lista de cursos del usuario
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var courses = await _courseService.GetByUserIdAsync(userId.Value);
            return Ok(courses);
        }

        // Busca el curso y cuánto ha avanzado el usuario en ese curso.
        [HttpGet("{id}")]
        public async Task<IActionResult> Show(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null) return NotFound();

            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> Store([FromBody] CreateCourseRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var course = await _courseService.CreateAsync(userId.Value, request);
            return CreatedAtAction(nameof(Show), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequestDto request)
        {
            var course = await _courseService.UpdateAsync(id, request);
            if (course == null) return NotFound();

            return Ok(course);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Destroy(int id)
        {
            var deleted = await _courseService.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            await _courseService.ArchiveAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            await _courseService.RestoreAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] CourseSearchRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var courses = await _courseService.SearchAsync(userId.Value, request);
            return Ok(courses);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var stats = await _courseService.GetStatsAsync(id, userId.Value);
            return Ok(stats);
        }


        //GESTION DE FECHAS DE EXAMENES

        [HttpGet("{id}/exam")]
        public async Task<IActionResult> GetExamDate(int id)
        {
            var exam = await _courseService.GetExamDateAsync(id);
            if (exam == null) return NotFound();

            return Ok(exam);
        }

        [HttpPut("{id}/exam")]
        public async Task<IActionResult> SetExamDate(int id, [FromBody] SetExamDateRequestDto request)
        {
            await _courseService.SetExamDateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}/exam")]
        public async Task<IActionResult> RemoveExamDate(int id)
        {
            await _courseService.RemoveExamDateAsync(id);
            return NoContent();
        }

        //CURSOS COMPARTIDOS
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(int id)
        {
            var members = await _courseService.GetMembersAsync(id);
            return Ok(members);
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddCourseMemberRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            await _courseService.AddMemberAsync(id, userId.Value, request);
            return NoContent();
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        {
            await _courseService.RemoveMemberAsync(id, userId);
            return NoContent();
        }

        [HttpPut("{id}/members/{userId}/role")]
        public async Task<IActionResult> ChangeMemberRole(int id, int userId, [FromBody] UpdateMemberRoleRequestDto request)
        {
            await _courseService.ChangeMemberRoleAsync(id, userId, request);
            return NoContent();
        }

    }
}
