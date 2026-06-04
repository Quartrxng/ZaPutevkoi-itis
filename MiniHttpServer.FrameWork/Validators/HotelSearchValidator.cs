using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork.Validators
{
    public class HotelSearchValidator
    {
        public HotelFilter ValidateAndBuildFilter(string name, string type, int duration, int tourists,
                                               int stars, string meal, string service, string detail, string rating,
                                               int startValue, int limit = 15)
        {
            string actualName = ValidateName(name, type);
            string actualType = ValidateType(type);
            string actualMeal = ValidateMeal(meal);
            string actualDetail = ValidateDetail(detail);
            int actualDuration = ValidateDuration(duration);
            int actualTourists = ValidateTourists(tourists);
            int actualStars = ValidateStars(stars);
            double actualRating = ValidateRating(rating);
            List<int> serviceList = ValidateServices(service);

            return new HotelFilter
            {
                Name = actualName,
                Type = actualType,
                Duration = actualDuration,
                TouristsCount = actualTourists,
                Stars = actualStars,
                Meal = actualMeal,
                Services = serviceList,
                Detail = actualDetail,
                StartValue = startValue,
                Rating = actualRating,
                Limit = limit
            };
        }

        private double ValidateRating(string rating)
        {
            if (string.IsNullOrWhiteSpace(rating))
                return 0.0;

            rating = rating.Trim().Replace(',', '.');

            if (double.TryParse(rating, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out double value))
            {
                return value;
            }

            return 0.0;
        }


        private string ValidateName(string name, string type)
        {
            string actualName = name ?? "";
            if (!string.IsNullOrEmpty(actualName) && actualName.Length > 3 && type == "hotel")
            {
                actualName = name.Substring(0, name.Length - 3);
            }
            return actualName;
        }

        private string ValidateType(string type)
        {
            return type ?? "Hotel";
        }

        private string ValidateMeal(string meal)
        {
            return meal ?? "Любой";
        }

        private string ValidateDetail(string detail)
        {
            string actualDetail = detail ?? "";
            if (!string.IsNullOrEmpty(actualDetail))
            {
                if (actualDetail.StartsWith("(") && actualDetail.EndsWith(")"))
                {
                    actualDetail = actualDetail.Substring(1, actualDetail.Length - 2);
                }
                actualDetail = actualDetail.Trim('"', '\'');
            }

            return actualDetail;
        }

        private int ValidateDuration(int duration)
        {
            return duration == 0 ? 1 : duration;
        }

        private int ValidateTourists(int tourists)
        {
            return tourists == 0 ? 3 : tourists;
        }

        private int ValidateStars(int stars)
        {
            return stars == 0 ? 1 : stars;
        }

        private List<int> ValidateServices(string service)
        {
            string actualService = service ?? "0";
            return actualService.Split(',')
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Select(s => int.TryParse(s, out int result) ? result : (int?)null)
                   .Where(x => x.HasValue)
                   .Select(x => x.Value)
                   .ToList();
        }
    }
}
