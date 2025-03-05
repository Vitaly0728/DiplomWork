namespace DiplomWork
{
    public class CofeAvtomat
    {
        public int Id { get; set; }
        public string Name { get; set; }        
        public string? Adres { get; set; }       
        public Location Location { get;  set; }
        public string PhotoURL { get; set; }               
        public CofeAvtomat(string _name  ,Location location, string _photoURL)
        {            
            Name = _name;            
            PhotoURL = _photoURL;
            Location = location;
        }
    }
}
