using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public class RoomDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }
        public PhotoFile PhotoFile { get; set; }
        public int PeopleCount { get; set; }
    }
}
