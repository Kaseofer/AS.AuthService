using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.DTOs
{
    public class GoogleAuthDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string GoogleId { get; set; }
    }
}
