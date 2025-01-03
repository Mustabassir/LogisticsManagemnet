using LogiticsManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiticsManagment.ViewModels
{
    public class DeliveryDateCalculator
    {
        public DateTime CalculateEstimatedDeliveryDate(string originCity, string destinationCity, string deliveryMethod)
        {
            DateTime estimatedDeliveryDate;

            if (originCity.Equals(destinationCity, StringComparison.OrdinalIgnoreCase))
            {
                // If origin city is the same as destination city, estimated delivery date is 7 days from current date
                estimatedDeliveryDate = DateTime.Today.AddDays(7);
            }
            else
            {
                // Calculate the estimated delivery date based on distance and delivery speed

                // Retrieve coordinates of origin and destination cities
                var originCoordinates = GetCityCoordinates(originCity);
                var destinationCoordinates = GetCityCoordinates(destinationCity);

                if (originCoordinates == null || destinationCoordinates == null)
                {
                    throw new ArgumentException("City coordinates not found.");
                }

                // Calculate distance between origin and destination cities
                var distance = CalculateGreatCircleDistance(
                    originCoordinates.Latitude, originCoordinates.Longitude,
                    destinationCoordinates.Latitude, destinationCoordinates.Longitude);

                // Determine delivery speed based on the delivery method
                double deliverySpeed = GetDeliverySpeed(deliveryMethod);

                // Calculate estimated days for delivery
                double estimatedDays = distance / deliverySpeed;

                // Calculate estimated delivery date by adding estimated days to current date
                estimatedDeliveryDate = DateTime.Today.AddDays((int)estimatedDays);
            }

            return estimatedDeliveryDate;
        }

        private CityCoordinates GetCityCoordinates(string cityName)
        {
            using (var db = new LogisticManagmentEntities1())
            {
                var city = db.Cities.SingleOrDefault(c => c.city_name == cityName);
                if (city != null)
                {
                    return new CityCoordinates
                    {
                        Latitude = (double)city.latitude,
                        Longitude = (double)city.longitude
                    };
                }
                return null;
            }
        }

        private double CalculateGreatCircleDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;

            double dLat = DegreeToRadians(lat2 - lat1);
            double dLon = DegreeToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreeToRadians(lat1)) * Math.Cos(DegreeToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private double DegreeToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private double GetDeliverySpeed(string deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case "Standard":
                    return 60.0;
                case "Express":
                    return 120.0;
                case "IntracityStandard":
                    return 10.0;
                case "IntracityExpress":
                    return 20.0;
                default:
                    return 60.0; // Default to standard if method is unknown
            }
        }
    }


    public class CityCoordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}