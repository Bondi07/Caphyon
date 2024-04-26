namespace ProiectCAPHYON.Models
{
    public class Author
    {
        public long Id { get; set; }
        public string Name { get; set; }    

        public List<RecipeDetails> Recipes { get; set; }
    }
}
