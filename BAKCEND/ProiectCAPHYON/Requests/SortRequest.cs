using ProiectCAPHYON.Enums;

namespace ProiectCAPHYON.Requests
{
    public class SortRequest
    {
        public SortOrder SortOrder { get; set; } = SortOrder.Asc;

        public string SortBy { get; set; } = "RecipeName";

    }
}
