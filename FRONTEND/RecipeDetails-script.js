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
            document.getElementById('skillLevel').textContent = recipeDetails.skillLevel;

           // Update ingredients list
            const ingredientsList = document.getElementById('ingredientsList');
            ingredientsList.innerHTML = ''; 
            if (recipeDetails.ingredients.length === 0) {
                const li = document.createElement('li');
                li.textContent = 'None'; 
                ingredientsList.appendChild(li);
            } else {
                recipeDetails.ingredients.forEach(ingredient => {
                    const li = document.createElement('li');
                    li.textContent = ingredient;
                    ingredientsList.appendChild(li);
                });
            }

            // Update collections list
            const collectionsList = document.getElementById('collections');
            collectionsList.innerHTML = ''; 
            if (recipeDetails.collections.length === 0) {
                const li = document.createElement('li');
                li.textContent = 'None'; 
                collectionsList.appendChild(li);
            } else {
                recipeDetails.collections.forEach(collection => {
                    const li = document.createElement('li');
                    li.textContent = collection;
                    collectionsList.appendChild(li);
                });
            }

            // Update keywords list
            const keywordsList = document.getElementById('keywords');
            keywordsList.innerHTML = ''; 
            if (recipeDetails.keywords.length === 0) {
                const li = document.createElement('li');
                li.textContent = 'None'; 
                keywordsList.appendChild(li);
            } else {
                recipeDetails.keywords.forEach(keyword => {
                    const li = document.createElement('li');
                    li.textContent = keyword;
                    keywordsList.appendChild(li);
                });
            }

            // Update diet types list
            const dietTypesList = document.getElementById('dietTypes');
            dietTypesList.innerHTML = ''; 
            if (recipeDetails.dietTypes.length === 0) {
                const li = document.createElement('li');
                li.textContent = 'None'; // or '/'
                dietTypesList.appendChild(li);
            } else {
                recipeDetails.dietTypes.forEach(dietType => {
                    const li = document.createElement('li');
                    li.textContent = dietType;
                    dietTypesList.appendChild(li);
                });
            }



            // Fetch similar recipes and update HTML
            fetchSimilarRecipes(recipeId)
                .then(similarRecipes => {
                    const similarRecipesList = document.getElementById('similarRecipes');
                    similarRecipesList.innerHTML = ''; 
                    similarRecipes.forEach(similarRecipe => {
                        const li = document.createElement('li');
                        const nameElement = document.createElement('span');
                        nameElement.textContent = similarRecipe.name;
                        const ingredientsElement = document.createElement('span');
                        ingredientsElement.textContent = `Number of Ingredients: ${similarRecipe.numberOfIngredients}`;
                        const skillLevelElement = document.createElement('span');
                        skillLevelElement.textContent = `Skill Level: ${similarRecipe.skillLevel}`;

                        // Append name, ingredients, and skill level elements to the list item
                        li.appendChild(nameElement);
                        li.appendChild(document.createElement('br')); // Add line break
                        li.appendChild(ingredientsElement);
                        li.appendChild(document.createElement('br')); // Add line break
                        li.appendChild(skillLevelElement);

                        // Append the list item to the list
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
