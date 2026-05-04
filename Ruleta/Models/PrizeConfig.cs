using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruleta.Models
{
    public class PrizeConfig
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; } // e.g., SMALL, MEDIUM, LARGE
        public int WinProbabilityWeight { get; set; }

        // These properties will be calculated dynamically later 
        // to tell the UI where exactly this prize is drawn on the circle
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
    }

    public class PrizeResult
    {
        public PrizeConfig WinningPrize { get; set; }

        // The exact degree where the animation MUST stop
        public double TargetAngle { get; set; }
    }
}