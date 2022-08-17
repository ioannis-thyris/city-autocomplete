namespace ElasticService
{
    public class CityDto
    {
        public long GeonameID { get; set; }
        public string AsciiName { get; set; }
        public string CountryNameEN { get; set; }
        public Coordinates Coordinates { get; set; }
    }

    public struct Coordinates
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
