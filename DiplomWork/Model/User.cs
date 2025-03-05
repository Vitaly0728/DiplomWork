namespace DiplomWork
{
    internal class User
    {        
        public int Reiting { get; set; }
        public long TelegramId { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public int Raiting {  get; set; }
        public User(long telegramId, string name, bool isAdmin,int raiting)
        {
            TelegramId = telegramId;
            Name = name;
            IsAdmin = isAdmin;
            Raiting = raiting;
        }
               

    }
}
