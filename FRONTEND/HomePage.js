/* HOME PAGE  */

/* PAGIANTION AND TABLE CONTENT */

let currentPage = 1;
const pageSize = 20; 
let recipeName = "";
let sortOrder = 0;
let sortBy = "RecipeName";
let ingredients = [];


function fetchAndRenderRecipes(pageNumber) {
    let http = new XMLHttpRequest();

    let data = {
        "pageNumber": pageNumber,
        "pageSize": pageSize,
        "recipeName": recipeName,
        "sortRequest": {
            "sortOrder": sortOrder,
            "sortBy": sortBy
        },
        "ingredients": ingredients
    };

    let jsonData = JSON.stringify(data);

    http.open('POST', 'https://localhost:7012/api/Recipes/All', true);
    http.setRequestHeader('Content-Type', 'application/json');
    http.send(jsonData);

    http.onload = function () {
        if (this.readyState == 4 && this.status == 200) {
            let recipes = JSON.parse(this.responseText);

            let out = `
                <h2>Table of Recipes:</h2>
                <div class="tableOfRecipes">
                    <table>
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Author</th>
                                <th>Number of Ingredients</th>
                                <th>Skill Level</th>
                            </tr>
                        </thead>
                        <tbody>
            `;

            for (let item of recipes) {
                out += `
                    <tr>
                        <td><a href="RecipeDetails.html?id=${item.id}">${item.name}</a></td>
                        <td><a href="Author.html?author=${encodeURIComponent(item.author)}">${item.author}</a></td>
                        <td>${item.numberOfIngredients}</td>
                        <td>${item.skillLevel}</td>
                    </tr>
                `;
            }

            out += `
                        </tbody>
                    </table>
                </div>
            `;

            document.querySelector(".tableOfRecipes").innerHTML = out;
        }
    }
}

/* NEXT AND PREVIOUS BUTTONS */

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

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

function updateNextButton() {
    const nextPageButton = document.getElementById('nextPage');
    if (currentPage === totalPages) {
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


document.getElementById('prevPage').addEventListener('click', () => {
    if (currentPage > 1) {
        currentPage--;
        fetchAndRenderRecipes(currentPage);
        updatePrevButton(); 
    }
});

document.getElementById('nextPage').addEventListener('click', () => {
    currentPage++;
    fetchAndRenderRecipes(currentPage);
    updatePrevButton(); 
});

fetchAndRenderRecipes(currentPage);
updatePrevButton(); 


/* AUTHOR COLUM PAGE  */
function handleAuthorClick(authorName) {
    window.location.href = `author.html?name=${encodeURIComponent(authorName)}`;
}


/* FILTER BY INGREDIENT AND SEARCH FUNCTION */

async function filterRecipes() {
    const filterByInput = document.getElementById('filterBy').value.trim();
    const listOfIngredients = filterByInput.split(',').map(ingredient => ingredient.trim());
    ingredients = listOfIngredients;
    recipeName = document.getElementById('recipeName').value.trim();

    fetchAndRenderRecipes(1);
}

window.addEventListener('load', async () => {
    const recipesData = await fetchRecipes();
    if (recipesData) {
        const ingredients = recipesData.ingredients;
        updateFilterInput(ingredients);
    }
});



/* TOP 5 ELEMENTS  */
window.onload = function () {
    fetchTopElements()
        .then(topElements => {
            updateTopElementsTable(topElements);
        })
        .catch(error => {
            console.error('Error fetching top elements:', error);
        });

    async function fetchTopElements() {
        try {
            const response = await fetch('https://localhost:7012/api/Recipes/TopElements');
            if (!response.ok) {
                throw new Error(`Failed to fetch top elements. Status: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error('Error fetching top elements:', error);
            throw error;
        }
    }

    function updateTopElementsTable(topElements) {
        updateIngredientsTable(topElements.ingredients);
        updateAuthorsTable(topElements.authors);
        updateRecipesTable(topElements.recipes);
    }

    function updateIngredientsTable(ingredients) {
        const topIngredientsTable = document.getElementById('topIngredientsTable');
        const topIngredientsBody = document.getElementById('topIngredientsBody');
        topIngredientsBody.innerHTML = '';

        ingredients.forEach((ingredient, index) => {
            const row = topIngredientsBody.insertRow();
            const rankCell = row.insertCell(0);
            rankCell.textContent = index + 1;
            const nameCell = row.insertCell(1);
            nameCell.textContent = ingredient.name;
            const recipeCountCell = row.insertCell(2);
            recipeCountCell.textContent = ingredient.recipeCount;
        });
    }

    function updateAuthorsTable(authors) {
        const topAuthorsTable = document.getElementById('topAuthorsTable');
        const topAuthorsBody = document.getElementById('topAuthorsBody');
        topAuthorsBody.innerHTML = '';
    
        authors.forEach((author, index) => {
            const row = topAuthorsBody.insertRow();
            const rankCell = row.insertCell(0);
            rankCell.textContent = index + 1;
    
            const nameCell = row.insertCell(1);
            const authorLink = document.createElement('a');
            authorLink.href = `Author.html?author=${encodeURIComponent(author.name)}`;
            authorLink.textContent = author.name;
            nameCell.appendChild(authorLink);
    
            const recipeCountCell = row.insertCell(2);
            recipeCountCell.textContent = author.recipeCount;
        });
    }
    
    function updateRecipesTable(recipes) {
        const topRecipesTable = document.getElementById('topRecipesTable');
        const topRecipesBody = document.getElementById('topRecipesBody');
        topRecipesBody.innerHTML = '';

        recipes.forEach((recipe, index) => {
            const row = topRecipesBody.insertRow();
            const rankCell = row.insertCell(0);
            rankCell.textContent = index + 1;

            const nameCell = row.insertCell(1);
            const recipeLink = document.createElement('a');
            recipeLink.href = `RecipeDetails.html?id=${recipe.id}`;
            recipeLink.textContent = recipe.name;
            nameCell.appendChild(recipeLink);

            const skillLevelCell = row.insertCell(2);
            skillLevelCell.textContent = recipe.skillLevel;

            const cookingTimeCell = row.insertCell(3);
            cookingTimeCell.textContent = recipe.cookingTime;

            const numberOfIngredientsCell = row.insertCell(4);
            numberOfIngredientsCell.textContent = recipe.numIngrediente;
        });
    }
};


