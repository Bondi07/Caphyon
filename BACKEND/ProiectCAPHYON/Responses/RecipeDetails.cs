namespace ProiectCAPHYON.Models
{
    public class RecipeDetails
    {
        public int PreparationTime { get; set; }    
        public string Name { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public string SkillLevel { get; set; }

        public float CookingTime { get; set;}

        public string AuthorName { get; set; }  

        public List<string> Collections { get; set; }
        public List<string> DietTypes { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> Ingredients { get; set; }

    }
}
