using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EShopCart.Models;

namespace EShopCart.Helpers
{
    public class JwtService
    {
        private readonly string _secretKey;

        public JwtService(IConfiguration config)//Injects the configuration settings
        {
            _secretKey = config["JwtSettings:SecretKey"];//Reads the JWT secret key from the configuration file appsettings.json
        }
        //Token Generation
        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler(); //JwtSecurityTokenHandler for creating and validating JWT tokens.
            var key = Encoding.UTF8.GetBytes(_secretKey); //Converts the secret key string into a byte array for secure operations

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor); 
            return tokenHandler.WriteToken(token);
            
        }
    }
}


