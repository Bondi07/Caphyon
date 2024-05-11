
/* AUTHOR */

let currentPage = 1;

// Function to parse query parameters from URL
function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, '\\$&');
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}

// Function to fetch and display author's recipes for a specific page
function fetchAndDisplayRecipes(pageNumber = 1, pageSize = 20) {
    const authorName = getParameterByName('author');
    if (!authorName) {
        console.error("Author name is missing.");
        return;
    }
    document.getElementById('authorName').textContent = authorName; // Set author's name in the heading

    // Use POST method to fetch recipes for the author with pagination parameters
    fetch('https://localhost:7012/api/Recipes/FilterByAuthor', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            authorName: authorName,
            pageNumber: pageNumber,
            pageSize: pageSize
        })
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`Failed to fetch recipes (status ${response.status})`);
        }
        return response.json();
    })
    .then(recipes => {
        // Display recipes for the current page
        displayRecipes(recipes);

        // Update next button based on the number of recipes
        updateNextButton(recipes.length);
    })
    .catch(error => {
        console.error('Error fetching recipes:', error);
    });
}

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Function to display recipes
function displayRecipes(recipes) {
    const recipeBody = document.getElementById('authorsRecipeBody');
    recipeBody.innerHTML = '';

    recipes.forEach(recipe => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${recipe.name}</td>
            <td>${recipe.numberOfIngredients}</td>
            <td>${recipe.skillLevel}</td>
        `;
        recipeBody.appendChild(row);
    });
}

// Function to update the state of the previous button
function updatePrevButton() {
    const prevPageButton = document.getElementById('prevPage');
    if (currentPage === 1) {
        prevPageButton.disabled = true;
        prevPageButton.style.backgroundColor = 'lightgrey';
        prevPageButton.style.cursor = 'not-allowed';
    } else {
        prevPageButton.disabled = false;
        prevPageButton.style.backgroundColor = '#4CAF50';
        prevPageButton.style.cursor = 'pointer';
    }
    scrollToTop();
}

// Function to update the state of the next button
function updateNextButton(recipeCount) {
    const nextPageButton = document.getElementById('nextPage');
    if (recipeCount < 20) {
        nextPageButton.disabled = true;
        nextPageButton.style.backgroundColor = 'lightgrey';
        nextPageButton.style.cursor = 'not-allowed';
    } else {
        nextPageButton.disabled = false;
        nextPageButton.style.backgroundColor = '#4CAF50';
        nextPageButton.style.cursor = 'pointer';
    }
    scrollToTop();
}

// Event listener for previous page button
document.getElementById('prevPage').addEventListener('click', () => {
    if (currentPage > 1) {
        currentPage--;
        fetchAndDisplayRecipes(currentPage);
        updatePrevButton();
    }
});

// Event listener for next page button
document.getElementById('nextPage').addEventListener('click', () => {
    currentPage++;
    fetchAndDisplayRecipes(currentPage);
    updatePrevButton();
});

// Update initial button states and fetch recipes when the page loads
updatePrevButton();
fetchAndDisplayRecipes();
    
