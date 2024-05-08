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
        })
        .catch(error => {
            console.error('Error fetching recipe details:', error);
        });
};

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
