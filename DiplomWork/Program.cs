using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DiplomWork
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SQLMachine sqlListMachine = new SQLMachine();
            ILocation locationUser = new Location(1, 2);
            User user = new User("Виталий",locationUser);

            Console.WriteLine($"Создан пользователь {user.Name} \nЕго координаты x = {locationUser.X} y = {locationUser.Y}");

            ILocation locationCoffe = new Location(5, 2);
            ILocation locationCoffe1 = new Location(2, 8);
            ILocation locationCoffe2 = new Location(15, 2);
            ILocation locationCoffe3 = new Location(3, 5);
            ILocation locationCoffe4 = new Location(1, 90);

            sqlListMachine.AddCoffeMachine(locationCoffe, "Кофе автомат");
            sqlListMachine.AddCoffeMachine(locationCoffe1, "Кофе автомат 1");
            sqlListMachine.AddCoffeMachine(locationCoffe2, "Кофе автомат 2");
            sqlListMachine.AddCoffeMachine(locationCoffe3, "Кофе автомат 3");
            sqlListMachine.AddCoffeMachine(locationCoffe4, "Кофе автомат 4");


            sqlListMachine.ListMachine();


            var locator = new CoffeeMachineLocator();
            var closestMachine = locator.FindClosestCoffeeMachine(user, sqlListMachine);

            Console.WriteLine($"Ближайший кофе автомат: {closestMachine.Name}");
        }
    }
}
