using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using test.Models;
using Microsoft.Extensions.Logging;



namespace test.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;//log

        public UsersController(AppDbContext context, IConfiguration configuration, ILogger<UsersController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterModel model)
        {
            _logger.LogInformation($"Registering user: {model.Username}");

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                _logger.LogWarning($"Username already exists: {model.Username}");
                return BadRequest("Username already exists.");
            }

             var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newUser = new UserModel
            {
                Username = model.Username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();
            _logger.LogInformation($"User registered successfully: {newUser.Username}"); 
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
        }

        [HttpPost("login")]
        public IActionResult Login(LoginModel model)
        {
            _logger.LogInformation($"Login attempt for user: {model.Username}");

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                _logger.LogWarning($"Invalid login credentials for user: {model.Username}"); 
                return Unauthorized("Invalid credentials.");
            }

            _logger.LogInformation($"User {model.Username} logged in successfully.");
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetUser(int id)
        {
            _logger.LogInformation($"Getting user with ID: {id}");
            var user = _context.Users.Find(id);

            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found.");
                return NotFound();
            }
            _logger.LogInformation($"User with ID: {id} found.");
            return Ok(user);
        }

        private string GenerateJwtToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
