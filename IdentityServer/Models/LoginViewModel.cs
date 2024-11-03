﻿namespace IdentityServer.Models
{
    public class LoginViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class LoginInputModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class VoiceAuthViewModel
    {
        public string Username { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class VoiceAuthInputModel
    {
        public string Username { get; set; }
        public string VoiceData { get; set; }
        public string ReturnUrl { get; set; }
    }

}