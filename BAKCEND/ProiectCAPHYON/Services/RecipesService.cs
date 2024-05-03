using Microsoft.VisualBasic;
using Neo4j.Driver;
using ProiectCAPHYON.Configurations;
using ProiectCAPHYON.Enums;
using ProiectCAPHYON.Models;
using ProiectCAPHYON.Requests;
using ProiectCAPHYON.Responses;
using System.Xml.Linq;
using Collection = ProiectCAPHYON.Models.Collection;

namespace ProiectCAPHYON.Services
{
    public class RecipesService : IRecipeService
    {
        private readonly IDriver _driver;

        private readonly Neo4jSettings _neo4jSettings;

        public RecipesService(Neo4jSettings neo4jSettings)
        {
            _neo4jSettings = neo4jSettings;
            _driver = GraphDatabase.Driver(neo4jSettings.Uri,
                AuthTokens.Basic(neo4jSettings.Username, neo4jSettings.Password));

        }


        public async Task<RecipeDetails> GetRecipeDetails(int recipeId)
        {
            var recipeDetails = new RecipeDetails();

            var query = $@"
                MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:COLLECTION]->(collection:Collection)
                MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:DIET_TYPE]->(dietType:DietType)
                MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:KEYWORD]->(keyword:Keyword)
                MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                MATCH (author:Author)-[:WROTE]->(recipe {{id: '{recipeId}'}}) 
                RETURN 
                TRIM(recipe.name) AS RecipeName,
                recipe.preparationTime AS PreparationTime,
                recipe.description AS Description,
                recipe.id AS Id,
                recipe.cookingTime AS CookingTime,
                recipe.skillLevel AS SkillLevel,
                COLLECT(DISTINCT collection.name) AS Collections,
                COLLECT(DISTINCT dietType.name) AS DietTypes,
                COLLECT(DISTINCT keyword.name) AS Keywords,
                COLLECT(DISTINCT ingredient.name) AS Ingredients,
                author.name AS AuthorName";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);
                var record = await result.SingleAsync();
                if (record != null)
                {
                    var name = record["RecipeName"].As<string>();
                    var id = record["Id"].As<long>();
                    var preparationTime = record["PreparationTime"].As<int>();
                    var description = record["Description"].As<string>();
                    var skillLevel = record["SkillLevel"].As<string>();
                    var cookingTime = record["CookingTime"].As<float>();

                    var collections = record["Collections"].As<List<string>>();
                    var dietTypes = record["DietTypes"].As<List<string>>();
                    var keywords = record["Keywords"].As<List<string>>();
                    var ingredients = record["Ingredients"].As<List<string>>();
                    var authorName = record["AuthorName"].As<string>();

                    recipeDetails = new RecipeDetails
                    {
                        Name = name,
                        Id = id,
                        PreparationTime = preparationTime,
                        Description = description,
                        SkillLevel = skillLevel,
                        CookingTime = cookingTime,
                        Collections = collections,
                        DietTypes = dietTypes,
                        Keywords = keywords,
                        Ingredients = ingredients,
                        AuthorName = authorName
                        
                    };
                }
            }

            return recipeDetails;
        }


        public async Task<List<Recipe>> GetRecipes(FilterByIngredientsRequest request)
        {

            var sortOrder = request.SortRequest.SortOrder == SortOrder.Asc ? "ASC" : "DESC";
            string ingredientsWhereClause = " ";
            string whereClause = " ";
            if (request.Ingredients.Count > 0)
            {
                ingredientsWhereClause = string.Join(" OR ", request.Ingredients.Select(ingredient => $"ingredient.name CONTAINS '{ingredient}'"));

            }
            string recipeWhereClause = !string.IsNullOrWhiteSpace(request.RecipeName) ? $" recipe.name CONTAINS '{request.RecipeName}'" : "";
            if (!string.IsNullOrWhiteSpace(ingredientsWhereClause))
            {
                whereClause += "WHERE " + ingredientsWhereClause;
                if (!string.IsNullOrWhiteSpace(recipeWhereClause))
                {
                    whereClause += " AND " + recipeWhereClause;
                }
            }
            else if (!string.IsNullOrWhiteSpace(recipeWhereClause))
            {
                whereClause += " WHERE " + recipeWhereClause;
            }


            var recipes = new List<Recipe>();
            var query = $@"
            MATCH (recipe:Recipe)
            MATCH (author:Author)-[:WROTE]->(recipe)
            MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
            WITH recipe, author, ingredient
            {ingredientsWhereClause}{recipeWhereClause}
            
            RETURN TRIM(recipe.name) AS RecipeName, recipe.id AS Id, recipe.skillLevel AS SkillLevel, author.name AS AuthorName, COUNT(ingredient) AS IngredientsCount
            ORDER BY {request.SortRequest.SortBy} {sortOrder}
            
            
            SKIP {request.PageNumber}
            LIMIT {request.PageSize}";

            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    var name = record["RecipeName"].As<string>();
                    var author = record["AuthorName"].As<string>();
                    var id = record["Id"].As<long>();
                    var skillLevel = record["SkillLevel"].As<string>();
                    var numberOfIngredients = record["IngredientsCount"].As<int>();

                    recipes.Add(new Recipe
                    {
                        Name = name,
                        Author = author,
                        Id = id,
                        NumberOfIngredients = numberOfIngredients,
                        SkillLevel = skillLevel
                    });
                });
            }
            return recipes;



        }

        public async Task<List<Recipe>> GetRecipesForAuthor(RecipeForAuthorRequest request)
        {
            string recipeWhereClause = !string.IsNullOrWhiteSpace(request.RecipeName) ? $" WHERE recipe.name CONTAINS '{request.RecipeName}' " : " ";

            var sortOrder = request.SortRequest.SortOrder == SortOrder.Asc ? "ASC" : "DESC";

            var recipes = new List<Recipe>();
            var matchQuery = $@"
                MATCH (a:Author {{ name: '{request.AuthorName}'}})-[:WROTE]->(recipe:Recipe)
                MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                
                ";
            if (!string.IsNullOrWhiteSpace(request.RecipeName))
            {
                matchQuery += recipeWhereClause;
            }
            var query = matchQuery + $@"
                RETURN TRIM(recipe.name) AS RecipeName, recipe.id AS Id, recipe.skillLevel AS SkillLevel, COUNT(ingredient) AS IngredientsCount
                ORDER BY RecipeName {sortOrder}
                SKIP {request.PageNumber}
                LIMIT {request.PageSize}";

            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    var name = record["RecipeName"].As<string>();
                    var id = record["Id"].As<long>();
                    var skillLevel = record["SkillLevel"].As<string>();
                    var numberOfIngredients = record["IngredientsCount"].As<int>();

                    recipes.Add(new Recipe
                    {
                        Name = name,
                        Author = request.AuthorName,
                        Id = id,
                        NumberOfIngredients = numberOfIngredients,
                        SkillLevel = skillLevel
                    });
                });
            }

            return recipes;
        }



        public async Task<List<Recipe>> GetSimilarRecipes(int recipeId)
        {
            var similareRecipes = new List<Recipe>();

            var query = $@"
                MATCH (targetRecipe:Recipe {{id: '101785'}})
                WITH targetRecipe.skillLevel AS skillLevel, [(targetRecipe)-[:CONTAINS_INGREDIENT]->(ingredient) | ingredient] AS ingredients
                WITH skillLevel, SIZE(ingredients) AS maxIngredients
                MATCH (recipe:Recipe)
                MATCH (author:Author)-[:WROTE]->(recipe)
                MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                WITH recipe, COLLECT(ingredient) AS ingredients, skillLevel, maxIngredients, author
                WHERE recipe.skillLevel = skillLevel AND SIZE(ingredients) <= maxIngredients
                RETURN TRIM(recipe.name) AS RecipeName, recipe.id AS Id, recipe.skillLevel AS SkillLevel, SIZE(ingredients) AS IngredientsCount, author.name AS AuthorName
                LIMIT 5;";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    var name = record["RecipeName"].As<string>();
                    var authorName = record["AuthorName"].As<string>();
                    var id = record["Id"].As<long>();
                    var skillLevel = record["SkillLevel"].As<string>();
                    var numberOfIngredients = record["IngredientsCount"].As<int>();

                    similareRecipes.Add(new Recipe
                    {
                        Name = name,
                        Author = authorName,
                        Id = id,
                        NumberOfIngredients = numberOfIngredients,
                        SkillLevel = skillLevel
                    });
                });
            }

            return similareRecipes;
        }



        public async Task<List<Recipe>> GetTopElements()
        {
            var similareRecipes = new List<Recipe>();

            var query = $@"
                MATCH (targetRecipe:Recipe {{id: '101785'}})
                WITH targetRecipe.skillLevel AS skillLevel, [(targetRecipe)-[:CONTAINS_INGREDIENT]->(ingredient) | ingredient] AS ingredients
                WITH skillLevel, SIZE(ingredients) AS maxIngredients
                MATCH (recipe:Recipe)
                MATCH (author:Author)-[:WROTE]->(recipe)
                MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                WITH recipe, COLLECT(ingredient) AS ingredients, skillLevel, maxIngredients, author
                WHERE recipe.skillLevel = skillLevel AND SIZE(ingredients) <= maxIngredients
                RETURN TRIM(recipe.name) AS RecipeName, recipe.id AS Id, recipe.skillLevel AS SkillLevel, SIZE(ingredients) AS IngredientsCount, author.name AS AuthorName
                LIMIT 5;";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    var name = record["RecipeName"].As<string>();
                    var authorName = record["AuthorName"].As<string>();
                    var id = record["Id"].As<long>();
                    var skillLevel = record["SkillLevel"].As<string>();
                    var numberOfIngredients = record["IngredientsCount"].As<int>();

                    similareRecipes.Add(new Recipe
                    {
                        Name = name,
                        Author = authorName,
                        Id = id,
                        NumberOfIngredients = numberOfIngredients,
                        SkillLevel = skillLevel
                    });
                });
            }

            return similareRecipes;
        }

        private async Task<List<Recipe>> GetMostComplexRecipe()
        {
            var recipes = new List<Recipe>();

            var query = $@"
                MATCH (recipe:Recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                WHERE recipe.skillLevel = ""A challenge""
                WITH recipe, COUNT(ingredient) AS numIngredients
                ORDER BY recipe.cookingTime DESC, numIngredients DESC
                LIMIT 5
                RETURN recipe.name AS RecipeName, recipe.skillLevel AS SkillLevel, recipe.cookingTime AS CookingTime, numIngredients AS NumIngredients;";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    var name = record["RecipeName"].As<string>();
                    var skillLevel = record["SkillLevel"].As<string>();
                    var cookingTime = record["CookingTime"].As<int>();
                    var numIngredients = record["NumIngredients"].As<int>();


                    recipes.Add(new Recipe
                    {
                        Name = name,
                        SkillLevel = skillLevel,
                        CookingTime = cookingTime,
                        NumIngrediente = numIngredients

                    });
                });
            }

            return recipes;
        }

        private async Task<List<ProlificAuthor>> GetMostProlificAuthors()
        {
            var authors = new List<ProlificAuthor>();

            var query = $@"
                MATCH (author:Author)-[:WROTE]->(recipe:Recipe)
                WITH author, COUNT(recipe) AS recipeCount
                ORDER BY recipeCount DESC
                RETURN author.name AS AuthorName, recipeCount AS RecipeCount
                LIMIT 5;";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    var name = record["AuthorName"].As<string>();
                    var recipeCount = record["RecipeCount"].As<int>();
                    

                    authors.Add(new ProlificAuthor
                    {
                        Name = name,
                        RecipeCount = recipeCount
                        
                    });
                });
            }

            return authors;


        }




    }
}
