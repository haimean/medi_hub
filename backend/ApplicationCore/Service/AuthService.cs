using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Data.Repository;
using MediHub.Web.HttpConfig;
using MediHub.Web.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediHub.Web.ApplicationCore.Service
{
    public class AuthService : HttpConfig.Service, IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository _repository;

        public AuthService(IConfiguration configuration, IRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> LoginAsync(string username, string password)
        {
            // Tìm người dùng trong database
            var user = (await _repository.FindAsync<UserEntity>(u => u.Username == username && u.IsDeleted != true));

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return BadRequest("Unauthorized");
            }

            // Cập nhật trạng thái đăng nhập
            user.LastLogin = DateTime.UtcNow;
            user.TokenExpiration = DateTime.UtcNow.AddHours(8); // Thiết lập thời gian hết hạn cho token
            user.IsTokenValid = true; // Đánh dấu token là hợp lệ
            await _repository.SaveChangeAsync();

            // Tạo token JWT
            return Ok(GenerateJwtToken(user));
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> LogoutAsync(string username)
        {
            // Tìm người dùng trong database
            var user = (await _repository.FindAsync<UserEntity>(u => u.Username == username && u.IsDeleted != true));

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Cập nhật trạng thái đăng xuất
            user.LastLogout = DateTime.UtcNow;
            user.IsTokenValid = false; // Đánh dấu token là không hợp lệ
            await _repository.SaveChangeAsync();

            return Ok();
        }

        /// <summary>
        /// Kiểm tra token còn hạn hay không
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Ok(message: "Invalid Token");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Giải mã token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // Không cho phép độ trễ
                };

                // Kiểm tra token
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Nếu token hợp lệ, kiểm tra thời gian hết hạn
                var jwtToken = (JwtSecurityToken)validatedToken;
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return Ok(message: "Token has expired");
                }

                // Kiểm tra trạng thái token trong cơ sở dữ liệu
                var userName = jwtToken.Claims.First(c => c.Type == "unique_name").Value.ToString();
                var user = await _repository.FindAsync<UserEntity>(u => u.Username == userName);
                if (user == null || !user.IsTokenValid)
                {
                    return Ok(message: "Token is no longer valid");
                }

                return Ok(true); // Token hợp lệ và chưa hết hạn
            }
            catch (SecurityTokenExpiredException)
            {
                return Ok(message: "Token has expired");
            }
            catch (Exception)
            {
                return Ok(message: "Invalid Token");
            }
        }

        #region Func support gen token and valid
        /// <summary>
        /// Tạo token JWT
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateJwtToken(UserEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, JsonConvert.SerializeObject(user.Role))
                }),
                Expires = DateTime.UtcNow.AddHours(8), // Token có thời hạn 8 tiếng
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Xác thực mật khẩu
        /// </summary>
        /// <param name="password"></param>
        /// <param name="storedHash"></param>
        /// <returns></returns>
        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // So sánh mật khẩu đã băm
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Băm mật khẩu
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string password)
        {
            // Băm mật khẩu trước khi lưu vào cơ sở dữ liệu
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        #endregion
    }
}