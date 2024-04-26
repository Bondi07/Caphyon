namespace ProiectCAPHYON.Requests
{
    public class FilterByIngredientsRequest : GetRecipesRequest
    {
        public List<string> Ingredients { get; set;} = new List<string>();


    }
}
