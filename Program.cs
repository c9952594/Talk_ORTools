using Google.OrTools.Sat;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Talk_ConstraintProgramming.Model;

namespace Talk_ConstraintProgramming
{
    class Program
    {
        static void Main(string[] args)
        {
            /*************/
            /* Load data */
            /*************/

            var ingredients = JsonConvert.DeserializeObject<Ingredient[]>(
                File.ReadAllText("Data/Ingredients.json")
            );

            var target = JsonConvert.DeserializeObject<Target>(
                File.ReadAllText("Data/Target.json")
            );


            /**************************/
            /* Constants / Dimensions */
            /**************************/

            /* This is needed because we need to work with integers
               10 calories per 100 grams = 0.1 calories / gram = Not an integer
               Multiple that by a factor though and we're good again. 
               We can multiple any target solution by the same factor. */
            const int decimalToIntegerFactor = 1000;

            var caloriesPerGramPerIngredient = ingredients.Select(ingredient =>
                (ingredient.Calories * decimalToIntegerFactor) / ingredient.NutritionalInformationWeight
            ).ToArray();

            var fatsPerGramPerIngredient = ingredients.Select(ingredient =>
                (ingredient.Fat * decimalToIntegerFactor) / ingredient.NutritionalInformationWeight
            ).ToArray();

            var carbsPerGramPerIngredient = ingredients.Select(ingredient =>
                (ingredient.Carbs * decimalToIntegerFactor) / ingredient.NutritionalInformationWeight
            ).ToArray();

            var proteinPerGramPerIngredient = ingredients.Select(ingredient =>
                (ingredient.Protein * decimalToIntegerFactor) / ingredient.NutritionalInformationWeight
            ).ToArray();


            /****************/
            /* Create model */
            /****************/

            var model = new CpModel();


            /*******************/
            /* Setup variables */
            /*******************/

            var ingredientIncluded = ingredients.Select(ingredient =>
                model.NewBoolVar(
                    $"include_{ingredient.Name}"
                )
            ).ToArray();

            var amountOfIngredients = ingredients.Select(ingredient =>
                model.NewIntVar(
                    0,
                    ingredient.MaximumAmount,
                    $"amountof_{ingredient.Name}"
                )
            ).ToArray();

            /******************************/
            /* Setup calculated variables */
            /******************************/

            var totalCalories = LinearExpr.ScalProd(
                amountOfIngredients,
                caloriesPerGramPerIngredient
            );

            var totalFatInGrams = LinearExpr.ScalProd(
                amountOfIngredients,
                fatsPerGramPerIngredient
            );

            var totalCarbsInGrams = LinearExpr.ScalProd(
                amountOfIngredients,
                carbsPerGramPerIngredient
            );

            var totalProteinInGrams = LinearExpr.ScalProd(
                amountOfIngredients,
                proteinPerGramPerIngredient
            );

            var totalAmountOfIngredients = LinearExpr.Sum(
                ingredientIncluded
            );

            /***************/
            /* Constraints */
            /***************/

            // for (var index = 0; index < ingredients.Length; index++)
            // {
            //     if (ingredient.CanInclude == false)
            //     {
            //         ingredientIncluded[index] = model.NewConstant(0);
            //     }

            //     var ingredient = ingredients[index];
            //     var includedVariable = ingredientIncluded[index];
            //     var amountVariable = amountOfIngredients[index];

            //     model.Add(amountVariable == 0).OnlyEnforceIf(includedVariable.Not());
            //     model.Add(amountVariable > ingredient.MinimumAmount).OnlyEnforceIf(includedVariable);

            //     // model.Add(amountVariable > 20).OnlyEnforceIf(includedVariable);
            //     // model.Add(amountVariable <= 100).OnlyEnforceIf(includedVariable);
            // }

            // model.AddLinearConstraint(totalAmountOfIngredients, target.MinimumNumberOfIngredients, target.MaximumNumberOfIngredients);

            model.Add(totalCalories >= target.MinimumCalories * decimalToIntegerFactor);

            // model.Add(totalFatInGrams * target.CarbPercentage == totalCarbsInGrams * target.FatPercentage);
            // model.Add(totalCarbsInGrams * target.ProteinPercentage == totalProteinInGrams * target.CarbPercentage);


            /********/
            /* Goal */
            /********/

            model.Minimize(totalCalories);

            /**********/
            /* Solver */
            /**********/

            var solver = new CpSolver();
            var status = solver.Solve(model);

            /********************/
            /* Display Solution */
            /********************/

            var ingredientsWithAnAmount =
                amountOfIngredients
                .Select((amountVariable, index) =>
                    new
                    {
                        Ingredient = ingredients[index],
                        Amount = solver.Value(amountVariable)
                    })
                .Where(m => m.Amount > 0)
                .ToArray();

            foreach (var ingredient in ingredientsWithAnAmount)
            {
                Console.WriteLine($"{ingredient.Amount}g {ingredient.Ingredient.Name}");
            }

            Console.WriteLine("");

            Console.WriteLine($"Total calories: {ingredientsWithAnAmount.Sum(m => (m.Amount * m.Ingredient.Calories) / m.Ingredient.NutritionalInformationWeight)}");
            Console.WriteLine($"Total fat: {ingredientsWithAnAmount.Sum(m => (m.Amount * m.Ingredient.Fat) / m.Ingredient.NutritionalInformationWeight)}");
            Console.WriteLine($"Total carbs: {ingredientsWithAnAmount.Sum(m => (m.Amount * m.Ingredient.Carbs) / m.Ingredient.NutritionalInformationWeight)}");
            Console.WriteLine($"Total protein: {ingredientsWithAnAmount.Sum(m => (m.Amount * m.Ingredient.Protein) / m.Ingredient.NutritionalInformationWeight)}");

            Debugger.Break();
        }
    }
}
