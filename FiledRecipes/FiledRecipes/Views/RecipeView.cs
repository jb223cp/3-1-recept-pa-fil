using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {
        public void Show(IRecipe recipe)
        {
            Console.Clear();
            ShowPanel(recipe.Name,ConsoleColor.White,ConsoleColor.DarkCyan);
            Console.WriteLine();
            Console.WriteLine("Ingredienser");
            Console.WriteLine("============");
            foreach (Ingredient item in recipe.Ingredients)
            {
                   Console.WriteLine("{0} {1} {2}",item.Amount ,item.Measure, item.Name);
            }
            Console.WriteLine();
            Console.WriteLine("Gör så här");
            Console.WriteLine("==========");

            int i = 1;
            foreach(string item in recipe.Instructions)
            {
                Console.WriteLine("<{0}>", i);
                Console.WriteLine(item);
                i++;
            }
        }

        public void Show(IEnumerable<IRecipe> recipes)
        {
            foreach (IRecipe item in recipes)
            {
                Show(item);
                ContinueOnKeyPressed();
            }
        }
    }
}
