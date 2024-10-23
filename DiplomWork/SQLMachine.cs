using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomWork
{
    internal class SQLMachine
    {

        public List<CofeAvtomat> cofeMachine = new List<CofeAvtomat>();

        public void AddCoffeMachine(ILocation location,string name)
        {
            CofeAvtomat machine= new CofeAvtomat(name, location);
            cofeMachine?.Add(machine);
            Console.WriteLine($"Кофе автомат \"{machine.Name}\" добавлен в базу данных!");
        }
        public void ListMachine()
        {
            if (cofeMachine != null)
            {
                foreach (var i in cofeMachine)
                {
                    Console.WriteLine(i.Name);
                }
            }
            else
            {
                throw new Exception("Список автоматов пуст");
            }
        }
    }
}
