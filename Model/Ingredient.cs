using System;
using System.Collections.Generic;
using System.Text;

namespace Talk_ConstraintProgramming.Model
{
    class Ingredient
    {
        public bool CanInclude { get; set; }
        public string Name { get; set; }
        public int NutritionalInformationWeight { get; set; }
        public int MinimumAmount { get; set; }
        public int MaximumAmount { get; set; }
        public int Calories { get; set; }
        public int Fat { get; set; }
        public int Carbs { get; set; }
        public int Protein { get; set; }
    }
}
