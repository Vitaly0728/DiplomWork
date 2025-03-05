namespace DiplomWork
{
    class CoffeAvtomatModderation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Adres { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }        
        public string PhotoURL { get; set; }
        public bool IsApproved { get; set; }
        public long ChatId { get; set; } 
        public int MessageId { get; set; } 

    }
}
