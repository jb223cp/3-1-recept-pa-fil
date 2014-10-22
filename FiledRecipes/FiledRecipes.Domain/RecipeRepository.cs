using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        /// <summary>
        /// Read recipes from text file
        /// </summary>
        public virtual void Load()
        {
            List<IRecipe> listOfRecipes = new List<IRecipe>();
            try
            {
                using (StreamReader reader = new StreamReader(_path))
                {
                    RecipeReadStatus status = RecipeReadStatus.Indefinite;
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {

                        switch (line)
                        {
                            case "":
                                continue;
                            case SectionRecipe:
                                status = RecipeReadStatus.New;
                                break;
                            case SectionIngredients:
                                status = RecipeReadStatus.Ingredient;
                                break;
                            case SectionInstructions:
                                status = RecipeReadStatus.Instruction;
                                break;
                            default:
                                switch (status)
                                {
                                    case RecipeReadStatus.New:
                                        listOfRecipes.Add(new Recipe(line));
                                        continue;
                                    case RecipeReadStatus.Ingredient:
                                        string[] ingredientsParts = line.Split(';');
                                        if (ingredientsParts.Length != 3)
                                            throw new FileFormatException();

                                        Ingredient ingredient = new Ingredient();
                                        ingredient.Amount = ingredientsParts[0];
                                        ingredient.Measure = ingredientsParts[1];
                                        ingredient.Name = ingredientsParts[2];

                                        listOfRecipes.Last().Add(ingredient);
                                        break;
                                    case RecipeReadStatus.Instruction:
                                        listOfRecipes.Last().Add(line);
                                        break;
                                    default:
                                        throw new FileFormatException();
                                }
                                break;
                        }
                    }
                    _recipes = listOfRecipes.OrderBy(r => r.Name).ToList();
                    IsModified = false;
                    OnRecipesChanged(EventArgs.Empty);
                }
            }
            catch (FileFormatException ex)
            {
                Console.WriteLine("Ett oväntat fel inträffade.\n{0}",
           ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ett oväntat fel inträffade.\n{0}",
                ex.Message);
            }
        }
        /// <summary>
        /// Method that writes new recipe into text file.
        /// Method implements functionality to save new recipe from console.
        /// </summary>
        public virtual void Save()
        {
            try
            {
                //Initialize StreamWriter object and write strings into text file

                using (StreamWriter writer = new StreamWriter(_path,true))
                {
                    writer.WriteLine(RecipeReadStatus.New);
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("\nAnge receptens namn: ");
                    Console.ResetColor();
                    writer.WriteLine(Console.ReadLine());
                    writer.WriteLine(RecipeReadStatus.Ingredient);

                    List<string> ingredienser = new List<string>();
                    string ingrediens = "";
                    string userInput = "";
                    do
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\nAnge ingrediensens mängd: ");
                        Console.ResetColor();
                        ingrediens = Console.ReadLine() + ";";
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\nAnge ingrediensens mått: ");
                        Console.ResetColor();
                        ingrediens += (Console.ReadLine()) + ";";

                        /* Checks if ingrediensens name is empty, and iterate through until it´s not empty*/
                        do
                        {
                            /* User friendly test */
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("\nAnge ingrediensens namn:");
                            Console.ResetColor();
                            /* end */

                            userInput = Console.ReadLine();
                            if (!String.IsNullOrEmpty(userInput))
                                ingrediens += userInput;
                        } while (String.IsNullOrEmpty(userInput));

                        ingredienser.Add(ingrediens);

                        /* User friendly test */
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\nTryck tangent för att fortsätta med ingredienser - [Esc Instruktioner]:");
                        Console.ResetColor();
                        /* end */

                    } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

                    foreach (String item in ingredienser)
                    {
                        writer.WriteLine(item);
                    }
                    writer.WriteLine(RecipeReadStatus.Instruction);
                    userInput = "";
                    do
                    {
                        /* User friendly test */
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\n\nSkriv en ny instruktion:");
                        Console.ResetColor();
                        /* end */
                        userInput = Console.ReadLine();
                        writer.WriteLine(userInput);

                        /* User friendly test */
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\nTryck tangent för att fortsätta med instruktioner - Esc, Spara ny recept]:");
                        Console.ResetColor();
                        /* end */

                    } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                    IsModified = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ett oväntat fel inträffade.\n{0}",
                ex.Message);
            }
        }
    }
}
