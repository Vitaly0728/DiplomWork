
namespace DiplomWork
{
    public  class Location
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public Location(double x, double y)
        {
            Latitude = x;
            Longitude = y;
        }        
    }
}
