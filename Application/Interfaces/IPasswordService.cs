using AgendaSalud.AuthService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaSalud.AuthService.Application.Interfaces
{
    public interface IPasswordService
    {
        Task<bool> ChangePasswordAsync(PasswordChangeDto dto);
        Task<bool> RequestResetAsync(PasswordResetRequestDto dto);
    }
}
