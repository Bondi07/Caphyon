﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using ProiectCAPHYON.Configurations;
using ProiectCAPHYON.Requests;
using ProiectCAPHYON.Services;

namespace ProiectCAPHYON.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }


      
        [HttpGet("Details")]
        public async Task<IActionResult> GetRecipeDetails(int recipeId)
        {
            return Ok(await _recipeService.GetRecipeDetails(recipeId));
        }

        

        [HttpPost("All")]
        public async Task<IActionResult> SearchRecipeByIngedients(GetRecipesRequest request)
        {
            return Ok(await _recipeService.GetRecipes(request));
        }

        [HttpPost("FilterByAuthor")]
        public async Task<IActionResult> FilterByAuthor(RecipeForAuthorRequest request)
        {
            return Ok(await _recipeService.GetRecipesForAuthor(request));
        }


        [HttpGet("Similar")]
        public async Task<IActionResult> SimilarRecipes(int recipeId)
        {
            return Ok(await _recipeService.GetSimilarRecipes(recipeId));
        }

        [HttpGet("TopElements")]
        public async Task<IActionResult> GetTopElements()
        {
            return Ok(await _recipeService.GetTopElements());
        }
    }
}
