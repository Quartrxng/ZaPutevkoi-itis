using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public class Service
    {
        public int Id { get; set; }
        public string Category_Name { get; set; } = string.Empty;
        public string Service_Name { get; set; } = string.Empty;
        public int Category_Order { get; set; }
        public int Service_Order { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}
