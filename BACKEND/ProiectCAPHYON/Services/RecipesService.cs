using Microsoft.VisualBasic;
using Neo4j.Driver;
using ProiectCAPHYON.Configurations;
using ProiectCAPHYON.Enums;
using ProiectCAPHYON.Models;
using ProiectCAPHYON.Requests;
using ProiectCAPHYON.Responses;
using System.Xml.Linq;

namespace ProiectCAPHYON.Services
{
    public class RecipesService : IRecipeService
    {
        private readonly IDriver _driver;

        public RecipesService(Neo4jSettings neo4jSettings)
        {
            _driver = GraphDatabase.Driver(neo4jSettings.Uri,
                AuthTokens.Basic(neo4jSettings.Username, neo4jSettings.Password));
        }


        public async Task<RecipeDetails> GetRecipeDetails(int recipeId)
        {
            var recipeDetails = new RecipeDetails();

            var query = $@"
                MATCH (recipe:Recipe {{id: '{recipeId}'}})
                OPTIONAL MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:COLLECTION]->(collection:Collection)
                OPTIONAL MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:DIET_TYPE]->(dietType:DietType)
                OPTIONAL MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:KEYWORD]->(keyword:Keyword)
                OPTIONAL MATCH (recipe:Recipe {{id: '{recipeId}'}})-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                OPTIONAL MATCH (author:Author)-[:WROTE]->(recipe {{id: '{recipeId}'}}) 
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
                    recipeDetails = new RecipeDetails
                    {
                        Name = record["RecipeName"].As<string>(),
                        Id = recipeId,
                        PreparationTime = record["PreparationTime"]?.As<int>() ?? 0,
                        Description = record["Description"].As<string>(),
                        SkillLevel = record["SkillLevel"].As<string>(),
                        CookingTime = record["CookingTime"]?.As<float>()?? 0,
                        Collections = record["Collections"].As<List<string>>(),
                        DietTypes = record["DietTypes"].As<List<string>>(),
                        Keywords = record["Keywords"].As<List<string>>(),
                        Ingredients = record["Ingredients"].As<List<string>>(),
                        AuthorName = record["AuthorName"].As<string>()
                    };
                }
            }
            return recipeDetails;
        }


        public async Task<List<Recipe>> GetRecipes(GetRecipesRequest request)
        {

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;

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
            {whereClause}
            
            RETURN TRIM(recipe.name) AS RecipeName, recipe.id AS Id, recipe.skillLevel AS SkillLevel, author.name AS AuthorName, COUNT(ingredient) AS IngredientsCount
            ORDER BY {request.SortRequest.SortBy} {sortOrder}
            
            
            SKIP {skip}
            LIMIT {take}";

            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    recipes.Add(new Recipe
                    {
                        Name = record["RecipeName"].As<string>(),
                        Author = record["AuthorName"].As<string>(),
                        Id = record["Id"].As<long>(),
                        NumberOfIngredients = record["IngredientsCount"].As<int>(),
                        SkillLevel = record["SkillLevel"].As<string>()
                    });
                });
            }
            return recipes;
        }

        public async Task<List<Recipe>> GetRecipesForAuthor(RecipeForAuthorRequest request)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;

            string recipeWhereClause = !string.IsNullOrWhiteSpace(request.RecipeName) ? $" WHERE recipe.name CONTAINS '{request.RecipeName}' " : " ";

            var sortOrder = request.SortRequest.SortOrder == SortOrder.Asc ? "ASC" : "DESC";

            var recipes = new List<Recipe>();
            var matchQuery = $@"
                MATCH (a:Author {{name: '{request.AuthorName}'}})
                OPTIONAL MATCH (a:Author {{ name: '{request.AuthorName}'}})-[:WROTE]->(recipe:Recipe)
                OPTIONAL MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                
                ";
            if (!string.IsNullOrWhiteSpace(request.RecipeName))
            {
                matchQuery += recipeWhereClause;
            }
            var query = matchQuery + $@"
                RETURN TRIM(recipe.name) AS RecipeName, recipe.id AS Id, recipe.skillLevel AS SkillLevel, COUNT(ingredient) AS IngredientsCount
                ORDER BY RecipeName {sortOrder}
                SKIP {skip}
                LIMIT {take}";

            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    recipes.Add(new Recipe
                    {
                        Name = record["RecipeName"].As<string>(),
                        Author = request.AuthorName,
                        Id = record["Id"].As<long>(),
                        NumberOfIngredients = record["IngredientsCount"].As<int>(),
                        SkillLevel = record["SkillLevel"].As<string>()
                    });
                });
            }

            return recipes;
        }



        public async Task<List<Recipe>> GetSimilarRecipes(int recipeId)
        {
            var similareRecipes = new List<Recipe>();

            var query = $@"
                MATCH (targetRecipe:Recipe {{id: '{recipeId}'}})
                WITH targetRecipe.skillLevel AS skillLevel, [(targetRecipe)-[:CONTAINS_INGREDIENT]->(ingredient) | ingredient] AS ingredients
                WITH skillLevel, SIZE(ingredients) AS maxIngredients
                MATCH (recipe:Recipe)
                MATCH (author:Author)-[:WROTE]->(recipe)
                OPTIONAL MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                WITH recipe, COLLECT(ingredient) AS ingredients, skillLevel, maxIngredients, author
                WHERE recipe.skillLevel = skillLevel AND SIZE(ingredients) <= maxIngredients
                RETURN TRIM(recipe.name) AS RecipeName, recipe.id AS Id, recipe.skillLevel AS SkillLevel, SIZE(ingredients) AS IngredientsCount, author.name AS AuthorName
                LIMIT 5;";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    similareRecipes.Add(new Recipe
                    {
                        Name = record["RecipeName"].As<string>(),
                        Author = record["AuthorName"].As<string>(),
                        Id = record["Id"].As<long>(),
                        NumberOfIngredients = record["IngredientsCount"].As<int>(),
                        SkillLevel = record["SkillLevel"].As<string>()
                    });
                });
            }

            return similareRecipes;
        }



        public async Task<TopElementsResponse> GetTopElements()
        {
            var recipes = await GetMostComplexRecipes();

            var ingredients = await GetMostCommonIngredients();

            var authors = await GetMostProlificAuthors();

            var topElements = new TopElementsResponse
            {
                Authors = authors,
                Ingredients = ingredients,
                Recipes = recipes
            };

            return topElements;
        }

        private async Task<List<ComplexRecipe>> GetMostComplexRecipes()
        {
            var recipes = new List<ComplexRecipe>();

            var query = $@"
                MATCH (recipe:Recipe)-[:CONTAINS_INGREDIENT]->(ingredient:Ingredient)
                WHERE recipe.skillLevel = ""A challenge""
                WITH recipe, COUNT(ingredient) AS numIngredients
                ORDER BY recipe.cookingTime DESC, numIngredients DESC
                LIMIT 5
                RETURN recipe.name AS RecipeName, recipe.skillLevel AS SkillLevel, recipe.cookingTime AS CookingTime, numIngredients AS NumIngredients, recipe.id AS RecipeId;";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    recipes.Add(new ComplexRecipe
                    {
                        Id = record["RecipeId"].As<int>(),
                        Name = record["RecipeName"].As<string>(),
                        SkillLevel = record["SkillLevel"].As<string>(),
                        CookingTime = record["CookingTime"].As<int>(),
                        NumIngrediente = record["NumIngredients"].As<int>()

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
                    authors.Add(new ProlificAuthor
                    {
                        Name = record["AuthorName"].As<string>(),
                        RecipeCount = record["RecipeCount"].As<int>()

                    });
                });
            }
            return authors;
        }


        private async Task<List<CommonIngredient>> GetMostCommonIngredients()
        {
            var ingredients = new List<CommonIngredient>();

            var query = $@"
                MATCH (r:Recipe)-[:CONTAINS_INGREDIENT]->(i:Ingredient)
                WITH i, COUNT(r) AS recipeCount
                ORDER BY recipeCount DESC
                RETURN i.name AS IngredientName, recipeCount AS RecipeCount
                LIMIT 5;";


            await using (var session = _driver.AsyncSession())
            {
                var result = await session.RunAsync(query);

                await result.ForEachAsync(record =>
                {
                    ingredients.Add(new CommonIngredient
                    {
                        Name = record["IngredientName"].As<string>(),
                        RecipeCount = record["RecipeCount"].As<int>()

                    });
                });
            }
            return ingredients;
        }

    }
}
