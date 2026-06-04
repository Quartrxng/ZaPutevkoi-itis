using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int Stars { get; set; }
        public string Rating { get; set; }
        public int Max_Size_Room { get; set; }
        public int Price { get; set; }
        public string About_Hotel { get; set; }
        public List<string> Meal { get; set; }
        public string Note { get; set; } = "Администрация отеля оставляет за собой право вносить любые изменения в концепцию отеля, в том числе о наборе платных/бесплатных услуг без предварительного уведомления. Мы просим предварительно уточнять интересующую Вас информацию.";
        public List<string> Image_Names { get; set; } = new List<string>();
        public List<string> Service_Names { get; set; } = new List<string>();
        public List<int> ServicesIds { get; set;} = new List<int>();
        public string RatingTag { get; set; }
        public string Card { get; set; }
        public List<Hotel_room> Hotel_Rooms { get; set; } = new List<Hotel_room>();
    }
}
