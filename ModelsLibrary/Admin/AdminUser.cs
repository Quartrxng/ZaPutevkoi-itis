using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Admin
{
    public class AdminUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public byte[] Password_Hash { get; set; } = null!;
        public byte[] Password_Salt { get; set; } = null!;
    }
}
