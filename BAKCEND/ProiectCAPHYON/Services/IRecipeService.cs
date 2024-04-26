using ProiectCAPHYON.Models;
using ProiectCAPHYON.Requests;

namespace ProiectCAPHYON.Services
{
    public interface IRecipeService
    {
        Task<RecipeDetails> GetRecipeDetails(int recipeId);

        Task<List<Recipe>> GetRecipes(FilterByIngredientsRequest request);

        Task<List<Recipe>> GetRecipesForAuthor(RecipeForAuthorRequest request);

        //Task<List<Recipe>> SortRecipesByNumberOfIngredients(SortRequest request);

        //Task<List<Recipe>> SortRecipesBySkillLevel(SortRequest request);


    }
}
