using ProiectCAPHYON.Models;
using ProiectCAPHYON.Requests;
using ProiectCAPHYON.Responses;

namespace ProiectCAPHYON.Services
{
    public interface IRecipeService
    {
        Task<RecipeDetails> GetRecipeDetails(int recipeId);

        Task<List<Recipe>> GetRecipes(GetRecipesRequest request);

        Task<List<Recipe>> GetRecipesForAuthor(RecipeForAuthorRequest request);

        Task<List<Recipe>> GetSimilarRecipes(int recipeId);

        Task<TopElementsResponse> GetTopElements();

    }
}
