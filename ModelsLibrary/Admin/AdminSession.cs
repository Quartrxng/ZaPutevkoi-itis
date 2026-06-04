using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Admin
{
    public class AdminSession
    {
        public int Id { get; set; } 
        public byte[] Token { get; set; } = null!;
        public int User_Id { get; set; }
        public DateTime Expires_At { get; set; }
    }
}
