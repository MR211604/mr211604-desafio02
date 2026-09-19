using System;
using System.Collections.Generic;
using System.Text;

namespace JwtAuthenticationManager.Models
{
    public class AuthenticationRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}