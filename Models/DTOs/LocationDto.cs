namespace Vintellitour_Framework.Models.DTOs
{
    public class LocationDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Address { get; set; }
        public CoordinatesDto Coordinates { get; set; }
    }

    public class CoordinatesDto
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
