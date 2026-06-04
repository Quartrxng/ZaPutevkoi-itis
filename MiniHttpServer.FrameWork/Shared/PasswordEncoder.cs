using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Shared
{
    public static class PasswordEncoder
    {
        public static byte[] GenerateSecureToken()
        {
            byte[] token = new byte[32]; // 256 бит
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(token);
            }
            return token;
        }
    }
}
