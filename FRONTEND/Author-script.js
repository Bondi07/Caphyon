// Function to get URL parameter by name
function getUrlParameter(name) {
    name = name.replace(/[[]/, '\\[').replace(/[\]]/, '\\]');
    var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
    var results = regex.exec(location.search);
    return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
}

// Get the author's name from the URL
const authorName = getUrlParameter('author');

// Display the author's name on the page
document.getElementById('authorName').textContent = authorName;

// Fetch all recipes by the author
fetch('https://localhost:7012/api/Recipes/FilterByAuthor', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        AuthorName: authorName,
        // Remove any limit on the number of recipes fetched
        // If there was a limit parameter in the original API, it's removed here
    })
})
.then(response => {
    if (!response.ok) {
        throw new Error(`Failed to fetch recipes (status ${response.status})`);
    }
    return response.json();
})
.then(recipes => {
    const recipeList = document.getElementById('recipeList');

    // Clear previous recipe list
    recipeList.innerHTML = '';

    if (recipes.length === 0) {
        const noRecipesMessage = document.createElement('p');
        noRecipesMessage.textContent = 'No recipes found for this author.';
        recipeList.appendChild(noRecipesMessage);
    } else {
        const ul = document.createElement('ul');
        recipes.forEach(recipe => {
            const li = document.createElement('li');
            li.textContent = recipe.name;
            ul.appendChild(li);
        });
        recipeList.appendChild(ul);
    }
})
.catch(error => {
    console.error('Error fetching recipes:', error);
});
