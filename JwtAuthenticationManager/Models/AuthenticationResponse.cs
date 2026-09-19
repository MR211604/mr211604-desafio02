using System;
using System.Collections.Generic;
using System.Text;

namespace JwtAuthenticationManager.Models
{
    public class AuthenticationResponse
    {
        public string username { get; set; }
        public string jwtToken { get; set; }
        public int expiresIn { get; set; }
    }
}