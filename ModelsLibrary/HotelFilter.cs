using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public class HotelFilter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Duration { get; set; }
        public int TouristsCount { get; set; }
        public double Rating { get; set; }
        public int Stars { get; set; }
        public string Meal { get; set; }
        public List<int> Services { get; set; }
        public string Detail { get; set; }
        public int StartValue { get; set; }
        public int Limit { get; set; } = 15;
    }
}
