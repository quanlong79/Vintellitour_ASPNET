using System.Collections.Generic;
using System.Text.Json;

namespace Vintellitour_Framework.Services.Geospatial
{
    public static class GeoJsonParser
    {
        public static List<(double lat, double lng)> ParsePolygonCoordinates(string geoJson)
        {
            using var doc = JsonDocument.Parse(geoJson);
            var root = doc.RootElement;

            var coords = root.GetProperty("geometry").GetProperty("coordinates");

            var polygonArray = coords[0];

            var polygon = new List<(double lat, double lng)>();

            foreach (var point in polygonArray.EnumerateArray())
            {
                double lng = point[0].GetDouble();
                double lat = point[1].GetDouble();
                polygon.Add((lat, lng));
            }

            return polygon;
        }
    }
}
