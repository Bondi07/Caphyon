namespace ProiectCAPHYON.Requests
{
    public class GetRecipesRequest : GetRecipesRequestBase
    {
        public List<string> Ingredients { get; set;} = new List<string>();


    }
}
