namespace Vintellitour_Framework.Models.DTOs
{
    public class LocationDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string DescriptionHistory { get; set; }  // Add this property
        public string Slug { get; set; }
        public int ProvinceGid { get; set; }
        public string OpenTime { get; set; }
        public string Price { get; set; }
        public List<string> Image { get; set; }
        public List<string> StreetViewUrls { get; set; }
        public List<string> Tags { get; set; }
        public CoordinatesDto Coordinates { get; set; }

        public class CoordinatesDto
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}