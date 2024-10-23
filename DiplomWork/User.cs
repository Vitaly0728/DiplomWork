using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomWork
{
    internal class User
    {
        public string Name { get;}
        public int Id { get;}
        public int Reiting { get; set; }
        public ILocation Location { get; set; }

        public User(string name,ILocation location )
        {
            Location = location;
            Name=name;
        }        

    }
}
