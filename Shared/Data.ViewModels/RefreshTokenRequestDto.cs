using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels
    {
        public class RefreshTokenRequestDto
        {
            public string RefreshToken { get; set; } = string.Empty;
        }
    }