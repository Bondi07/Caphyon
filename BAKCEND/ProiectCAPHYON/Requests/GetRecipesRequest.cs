using ProiectCAPHYON.Enums;

namespace ProiectCAPHYON.Requests
{
    public class GetRecipesRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string RecipeName { get; set; } = "";

        public SortRequest SortRequest { get; set; } = new SortRequest();

    }
}
