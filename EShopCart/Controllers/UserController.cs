using Microsoft.AspNetCore.Mvc;
using EShopCart.Models;
using EShopCart.Helpers;
using EShopCart.Repositories;
using AutoMapper;
using EShopCart.DTOs;
using Microsoft.AspNetCore.Authorization;
using MimeKit;
using MailKit.Security;
using MimeKit.Text;
using System.Text.RegularExpressions;

namespace EShopCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, JwtService jwtService, IMapper mapper,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _mapper = mapper;
            _logger = logger;
        }

        #region Registration

        // Registration Endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest(new { message = "User data is required." });
            }

            if (await _userRepository.UserExistsAsync(userDto.Username))
            {
                return BadRequest(new { message = "Username already exists." });
            }

            // Validate email using regular expression
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(userDto.Email))
            {
                return BadRequest(new { message = "Invalid email address." });
            }
 
            // Validate role
            if (userDto.Role.ToLower() != "customer" && userDto.Role.ToLower() != "merchant")
            {
                return BadRequest(new { message = "Role must be either 'customer' or 'merchant'." });
            }

            // Hash password before saving
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // Map DTO to User entity and set password hash
        
        
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = hashedPassword;

            // Add the new user to the repository
            await _userRepository.AddUserAsync(user);

            // Send the welcome email
            try
            {
                await SendWelcomeEmail(userDto.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email.");
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }

            System.Console.WriteLine("Logging in......");

            return Ok(new { message = "User registered successfully. A confirmation email has been sent." });
        }

        // Private method to send a registration email
        private async Task SendWelcomeEmail(string recipientEmail)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("madhurpugliya10@gmail.com")); // Sender's email
            email.To.Add(MailboxAddress.Parse(recipientEmail));  // Recipient email
            email.Subject = "Registration Successful on EShopCart";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = "<h1>Registration Successful!</h1><p>Welcome to EShopCart! Your registration has been successfully completed.</p>"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls); // SMTP Gmail server
            await smtp.AuthenticateAsync("madhurpugliya10@gmail.com", "hpqtovtyovojlnrw"); // Gmail credentials (use app password)
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        #endregion

        #region Login

        // Login Endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }

        #endregion

        #region Profile

        // Profile Endpoint (for authenticated customers only)
        [HttpGet("profile")]
        [AllowAnonymous]

        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var userDto = _mapper.Map<UserDto>(user); // Map entity to DTO for response
            return Ok(userDto);
        }

        #endregion


    }
}
