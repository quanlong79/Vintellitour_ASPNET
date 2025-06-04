using System;
using System.Collections.Generic;

namespace Vintellitour_Framework.Services.Geospatial
{
    public static class CustomGeospatialService
    {
        // Ray Casting Algorithm - Kiểm tra điểm trong polygon
        public static bool IsPointInPolygon(double lat, double lng, List<(double lat, double lng)> polygon)
        {
            bool inside = false;
            int j = polygon.Count - 1;

            for (int i = 0; i < polygon.Count; i++)
            {
                double xi = polygon[i].lng;
                double yi = polygon[i].lat;
                double xj = polygon[j].lng;
                double yj = polygon[j].lat;

                if (((yi > lat) != (yj > lat)) &&
                    (lng < (xj - xi) * (lat - yi) / (yj - yi) + xi))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }
    }
}
