using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaSalud.AuthService.Application.DTOs
{
    public class PasswordChangeDto
    {
        public string ResetToken { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

}
