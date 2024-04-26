using ProiectCAPHYON.Enums;

namespace ProiectCAPHYON.Requests
{
    public class GetRecipesRequest
    {
        public int PageNumber { get; set; }
        
        public int PageSize { get; set; }

        public string RecipeName { get; set; } = "";

        public SortOptions SortOptions { get; set; } = SortOptions.Asc;

    }
}
