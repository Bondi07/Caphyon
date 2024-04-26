namespace ProiectCAPHYON.Models
{
    public class RecipeDetails
    {
        public int PreparationTime { get; set; }    
        public string Name { get; set; }

        public string Description { get; set; }

        public long Id { get; set; }

        public string SkillLevel { get; set; }

        public float CookingTime { get; set;}

        public List<Collection> Collections { get; set; }
        public List<DietType> DietTypes { get; set; }
        public List<Keyword> Keywords { get; set; }
        public List<Ingredient> Ingredients { get; set; }

    }
}
