using MediHub.Web.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediHub.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _authService.LoginAsync(loginRequest.Username, loginRequest.Password);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string userName)
        {
            var result = await _authService.LogoutAsync(userName);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Kiểm tra token còn hạn hay không
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        [HttpGet("check-token")]
        public async Task<IActionResult> CheckToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { valid = false, message = "Token is required." });
            }

            var isValid = await _authService.ValidateToken(token); // Giả sử bạn có phương thức ValidateToken trong IAuthService

            return StatusCode((int)isValid.StatusCode, isValid);
        }
    }

    /// <summary>
    /// Yêu cầu đăng nhập
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}