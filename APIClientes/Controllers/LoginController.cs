
using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace APIClientes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public IActionResult Login(AuthenticationRequest request)
        {
            var handler = new JwtTokenHandler();
            var authResponse = handler.generateJwtToken(request);

            if (authResponse == null)
            {
                return BadRequest("Credenciales invalidas");
            }

            return Ok(authResponse);
        }
    }
}
