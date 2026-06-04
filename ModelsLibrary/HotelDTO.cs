using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public class HotelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int Stars { get; set; }
        public double Rating { get; set; }
        public string About_Hotel { get; set; }
        public string Meal { get; set; }
        public string Note { get; set; } = "Администрация отеля оставляет за собой право вносить любые изменения в концепцию отеля, в том числе о наборе платных/бесплатных услуг без предварительного уведомления. Мы просим предварительно уточнять интересующую Вас информацию.";
        public string Image_Names { get; set; }
        public string Card { get; set; }
    }
}
