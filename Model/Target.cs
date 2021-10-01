using System;
using System.Collections.Generic;
using System.Text;

namespace Talk_ConstraintProgramming.Model
{
    class Target
    {
        public int MinimumCalories { get; set; }
        public int FatPercentage { get; set; }
        public int CarbPercentage { get; set; }
        public int ProteinPercentage { get; set; }
        public int MinimumNumberOfIngredients { get; set; }
        public int MaximumNumberOfIngredients { get; set; }
    }
}
