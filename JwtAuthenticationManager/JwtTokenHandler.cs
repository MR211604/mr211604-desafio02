using JwtAuthenticationManager.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthenticationManager
{
    public class JwtTokenHandler
    {
        public const string JWT_SECURITY_KEY = "ESTOESUNACLAVEBASTANTESEGURAPARAPODERHASHEARCORRECTAMENTEELTOKENWAOS";
        private const int JWT_TOKEN_VALIDITY_MINS = 20;

        private readonly List<UserAccount> _userAccounts;

        public JwtTokenHandler()
        {
            _userAccounts = new List<UserAccount>
            {
                new UserAccount { userName = "test1", password = "password1" },
                new UserAccount { userName = "test2", password = "password2" }
            };
        }

        public AuthenticationResponse? generateJwtToken(AuthenticationRequest authenticationRequest)
        {
            if (string.IsNullOrEmpty(authenticationRequest.username) || string.IsNullOrEmpty(authenticationRequest.password))
            {
                return null;
            }

            //Validando que el usuario y la contraseña sean correctos
            var userAccount = _userAccounts.FirstOrDefault(u => u.userName == authenticationRequest.username && u.password == authenticationRequest.password);

            if (userAccount == null)
            {
                return null;
            }

            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(JWT_TOKEN_VALIDITY_MINS);

            var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);

            var claimsIdentity = new ClaimsIdentity(new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Name, authenticationRequest.username)
            });

            var signinCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature
                );

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpiryTimeStamp,
                SigningCredentials = signinCredentials,
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            return new AuthenticationResponse
            {
                username = userAccount.userName,
                expiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds,
                jwtToken = token
            };

        }
    }
}