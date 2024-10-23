using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomWork
{
    public struct CofeAvtomat
    {

        public ILocation Location { get; set; }
        public string Name { get; set; }
        int Id { get;}

        public CofeAvtomat(string _name,ILocation location)
        {
            Location = location;
            Name = _name;
        }

        
    }
}
