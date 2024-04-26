using Microsoft.AspNetCore.Routing;
using Microsoft.VisualBasic;
using Neo4j.Driver;
using ProiectCAPHYON.Configurations;
using ProiectCAPHYON.Enums;
using ProiectCAPHYON.Models;
using ProiectCAPHYON.Requests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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
                MATCH (recipe)

                WHERE recipe.id = '{recipeId}'

                RETURN TRIM(recipe.name) AS RecipeName,
                        recipe.preparationTime AS PreparationTime,
                        recipe.description AS Description,
                        recipe.id AS Id,
                        recipe.cookingTime AS CookingTime,
                        recipe.skillLevel AS SkillLevel";

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

                    recipeDetails = new RecipeDetails
                    {
                        Name = name,
                        Id = id,
                        PreparationTime = preparationTime,
                        Description = description,
                        SkillLevel = skillLevel,
                        CookingTime = cookingTime
                    };
                }
            }

            return recipeDetails;
        }


        public async Task<List<Recipe>> GetRecipes(FilterByIngredientsRequest request)
        {

            var sortOrder = request.SortOptions == SortOptions.Asc ? "ASC" : "DESC";
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
            ORDER BY RecipeName {sortOrder}
            
            
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

            var sortOrder = request.SortOptions == SortOptions.Asc ? "ASC" : "DESC";

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

        //public async Task<List<Recipe>> SortRecipesByNumberOfIngredients(SortRequest request);

        //public async Task<List<Recipe>> SortRecipesBySkillLevel(SortRequest request);
    }
}
