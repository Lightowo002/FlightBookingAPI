using FlightBookingAPI.Models;
using FlightBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            try
            {
                await _authService.Registrar(usuario);
                return Ok(new { message = "Usuario registrado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _authService.Login(request.Correo, request.Contraseña);
            if (usuario == null)
            {
                return Unauthorized(new { message = "Correo o contraseña incorrectos" });
            }
            return Ok(new { message = "Login exitoso", usuario });
        }

        [HttpPost("google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var usuario = await _authService.LoginConGoogle(request.Token);
                return Ok(new { message = "Login con Google exitoso", usuario });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.SolicitarRecuperacion(request.Correo);
                return Ok(new { message = "Te enviamos un correo con las instrucciones" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authService.RestablecerContraseña(request.Token, request.NuevaContraseña);
                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
