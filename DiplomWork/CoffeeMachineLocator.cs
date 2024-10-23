using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomWork
{
    internal class CoffeeMachineLocator
    {
        public CofeAvtomat FindClosestCoffeeMachine(User user, SQLMachine coffeeMachines)
        {
            if (coffeeMachines == null || coffeeMachines.cofeMachine.Count == 0)
                throw new ArgumentException("Список кофе автоматов не может быть пустым.");

            CofeAvtomat closestMachine = coffeeMachines.cofeMachine[0];
            double minDistance = CalculateDistance(user.Location, closestMachine.Location);

            foreach (var machine in coffeeMachines.cofeMachine)
            {
                double distance = CalculateDistance(user.Location, machine.Location);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestMachine = machine;
                }
            }

            return closestMachine;
        }

        private double CalculateDistance(ILocation loc1, ILocation loc2)
        {
            return Math.Sqrt(Math.Pow(loc1.X - loc2.X, 2) + Math.Pow(loc1.Y - loc2.Y, 2));
        }

    }
}
