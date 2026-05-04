using Ruleta.Models;
using Ruleta.Models;
using System.Collections.Generic;

namespace Ruleta.Services
{
    public static class MockDatabaseDao
    {
        public static List<PrizeConfig> GetPrizesForCategory(string category)
        {
            // Simulating a query: SELECT * FROM Inventory WHERE Category = @category AND Stock > 0

            var prizes = new List<PrizeConfig>();

            if (category == "SMALL") // For tickets $100 - $500
            {
                prizes.Add(new PrizeConfig { ProductId = 1, ProductName = "Chicle", WinProbabilityWeight = 100 });
                prizes.Add(new PrizeConfig { ProductId = 2, ProductName = "Paleta", WinProbabilityWeight = 80 });
                prizes.Add(new PrizeConfig { ProductId = 3, ProductName = "Papas", WinProbabilityWeight = 40 });
                prizes.Add(new PrizeConfig { ProductId = 4, ProductName = "Refresco Lata", WinProbabilityWeight = 10 });
            }
            else if (category == "LARGE") // For tickets > $1000
            {
                prizes.Add(new PrizeConfig { ProductId = 5, ProductName = "Six de Cerveza", WinProbabilityWeight = 60 });
                prizes.Add(new PrizeConfig { ProductId = 6, ProductName = "Botella de Vino", WinProbabilityWeight = 20 });
                prizes.Add(new PrizeConfig { ProductId = 7, ProductName = "Bolsa de Carbón", WinProbabilityWeight = 40 });
                prizes.Add(new PrizeConfig { ProductId = 8, ProductName = "Despensa Básica", WinProbabilityWeight = 5 });
            }

            return prizes;
        }
    }
}