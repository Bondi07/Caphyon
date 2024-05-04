using ProiectCAPHYON.Models;
using System.Security.Cryptography.X509Certificates;

namespace ProiectCAPHYON.Responses
{
    public class TopElementsResponse
    {
        public List<CommonIngredient> Ingredients { get; set; }

        public List<ProlificAuthor> Authors{ get; set; }

        public List<ComplexRecipe> Recipes { get; set; }
    }
}
