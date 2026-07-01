using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Usuario no autenticado.");
            return userId;
        }

        protected IActionResult Forbidden(string message = "No tenés permiso para acceder a este recurso.")
        {
            return StatusCode(403, new { statusCode = 403, message });
        }
    }
}
