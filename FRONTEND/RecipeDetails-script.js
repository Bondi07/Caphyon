window.onload = function () {
    // Extract recipe ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    const recipeId = urlParams.get('id');

    // Fetch recipe details based on ID
    fetchRecipeDetails(recipeId)
        .then(recipeDetails => {
            // Update HTML with recipe details
            document.getElementById('recipeName').textContent = recipeDetails.name;
            document.getElementById('description').textContent = recipeDetails.description;
            document.getElementById('cookingTime').textContent = recipeDetails.cookingTime;
            document.getElementById('preparationTime').textContent = recipeDetails.preparationTime;

            // Update ingredients list
            const ingredientsList = document.getElementById('ingredientsList');
            ingredientsList.innerHTML = ''; // Clear previous list
            recipeDetails.ingredients.forEach(ingredient => {
                const li = document.createElement('li');
                li.textContent = ingredient;
                ingredientsList.appendChild(li);
            });

            // Update collections list
            const collectionsList = document.getElementById('collections');
            collectionsList.innerHTML = ''; // Clear previous list
            recipeDetails.collections.forEach(collection => {
                const li = document.createElement('li');
                li.textContent = collection;
                collectionsList.appendChild(li);
            });

            // Update keywords list
            const keywordsList = document.getElementById('keywords');
            keywordsList.innerHTML = ''; // Clear previous list
            recipeDetails.keywords.forEach(keyword => {
                const li = document.createElement('li');
                li.textContent = keyword;
                keywordsList.appendChild(li);
            });

            // Update diet types list
            const dietTypesList = document.getElementById('dietTypes');
            dietTypesList.innerHTML = ''; // Clear previous list
            recipeDetails.dietTypes.forEach(dietType => {
                const li = document.createElement('li');
                li.textContent = dietType;
                dietTypesList.appendChild(li);
            });

            // Fetch similar recipes and update HTML
            fetchSimilarRecipes(recipeId)
                .then(similarRecipes => {
                    const similarRecipesList = document.getElementById('similarRecipes');
                    similarRecipesList.innerHTML = ''; // Clear previous list
                    similarRecipes.forEach(similarRecipe => {
                        const li = document.createElement('li');
                        const similarityFactor = calculateSimilarity(recipeDetails, similarRecipe) * 100; // Calculate similarity factor
                        li.textContent = `${similarRecipe.name} (Ingredients: ${similarRecipe.numberOfIngredients}, Skill Level: ${similarRecipe.skillLevel}, Similarity: ${similarityFactor.toFixed(2)}%)`; // Display number of ingredients, skill level, and similarity factor
                        similarRecipesList.appendChild(li);
                    });
                })
                .catch(error => {
                    console.error('Error fetching similar recipes:', error);
                });
        })
        .catch(error => {
            console.error('Error fetching recipe details:', error);
        });
};

// Function to calculate similarity factor
function calculateSimilarity(recipe1, recipe2) {
    // Similarity based on number of ingredients
    const ingredientSimilarity = 1 - Math.abs(recipe1.ingredients.length - recipe2.numberOfIngredients) / Math.max(recipe1.ingredients.length, recipe2.numberOfIngredients);
    // Similarity based on skill level
    const skillLevelSimilarity = recipe1.skillLevel === recipe2.skillLevel ? 1 : 0;
    // You can adjust the weights according to your preference
    const ingredientWeight = 0.6;
    const skillLevelWeight = 0.4;
    // Combine similarities with weights
    return (ingredientWeight * ingredientSimilarity) + (skillLevelWeight * skillLevelSimilarity);
}

// Function to fetch recipe details based on ID
async function fetchRecipeDetails(recipeId) {
    try {
        const response = await fetch(`https://localhost:7012/api/Recipes/Details?recipeId=${recipeId}`);
        if (!response.ok) {
            throw new Error(`Failed to fetch recipe details. Status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching recipe details:', error);
        throw error;
    }
}

// Function to fetch similar recipes based on ID
async function fetchSimilarRecipes(recipeId) {
    try {
        const response = await fetch(`https://localhost:7012/api/Recipes/Similar?recipeId=${recipeId}`);
        if (!response.ok) {
            throw new Error(`Failed to fetch similar recipes. Status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching similar recipes:', error);
        throw error;
    }
}
