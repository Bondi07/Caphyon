
/* AFISAREA RECETELOR */
// let http = new XMLHttpRequest();

// let data = {
//     "pageNumber": 1,
//     "pageSize": 20
// };

// let jsonData = JSON.stringify(data);

// http.open('POST', 'https://localhost:7012/api/Recipes/All', true);
// http.setRequestHeader('Content-Type', 'application/json');
// http.send(jsonData); 

// http.onload = function(){

//     if(this.readyState == 4 && this.status == 200){

//         let recipes = JSON.parse(this.responseText);

//         let out = `
//             <div class="tableOfRecipes">
//                 <table>
//                     <thead>
//                         <tr>
//                             <th>Name</th>
//                             <th>Author</th>
//                             <th>Number of Ingredients</th>
//                             <th>Skill Level</th>
//                         </tr>
//                     </thead>
//                     <tbody>
//         `;
        
//         for(let item of recipes){
//             out += `
//                 <tr>
//                     <td>${item.name}</td>
//                     <td>${item.author}</td>
//                     <td>${item.numberOfIngredients}</td>
//                     <td>${item.skillLevel}</td>
//                 </tr>
//             `; 
//         }

//         out += `
//                     </tbody>
//                 </table>
//             </div>
//         `;
        
//         document.querySelector(".tableOfRecipes").innerHTML = out;
//     }
// }


/* ####################################################################################################### */

/* PAGIANTION AND TABLE CONTENT */

let currentPage = 1;
const pageSize = 20; 

function fetchAndRenderRecipes(pageNumber) {
    let http = new XMLHttpRequest();

    let data = {
        "pageNumber": pageNumber,
        "pageSize": pageSize
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



/* SEARCH FUNCTION */



document.addEventListener('DOMContentLoaded', function() {
    async function searchSimilarRecipes() {
        const recipeName = document.getElementById('recipeName').value.trim();

        const requestBody = {
            RecipeName: recipeName
        };

        const response = await fetch('https://localhost:7012/api/Recipes/All', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        const recipes = await response.json();

        const recipeBody = document.getElementById('recipeBody');
        if (recipeBody) {
            recipeBody.innerHTML = ''; 

            recipes.forEach(recipe => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td><a href="RecipeDetails.html?id=${recipe.Id}">${recipe.Name}</a></td>
                    <td><a href="Author.html?author=${encodeURIComponent(recipe.author)}">${recipe.author}</a></td>
                    <td>${recipe.NumberOfIngredients}</td>
                    <td>${recipe.SkillLevel}</td>
                `;
                recipeBody.appendChild(row);
            });
        } else {
            console.error("Element with ID 'recipeBody' not found.");
        }
    }

    // Add event listener to search button
    document.getElementById('searchButton').addEventListener('click', searchSimilarRecipes);

});



/* AUTHOR COLUM PAGE  */

// Function to handle author click event
function handleAuthorClick(authorName) {
    // Redirect to author page with authorName parameter
    window.location.href = `author.html?name=${encodeURIComponent(authorName)}`;
}



/* FILTER BY INGREDIENT  */


// Function to update the table with recipes
function updateRecipeTable(recipes) {
    const tableBody = document.getElementById('recipeBody');
    if (!tableBody) {
        console.error('Table body element not found');
        return;
    }
    
    // Remove existing rows
    while (tableBody.firstChild) {
        tableBody.removeChild(tableBody.firstChild);
    }

    recipes.forEach(recipe => {
        const row = document.createElement('tr');

        const nameCell = document.createElement('td');
        nameCell.textContent = recipe.Name;
        row.appendChild(nameCell);

        const authorCell = document.createElement('td');
        const authorLink = document.createElement('a');
        authorLink.textContent = recipe.Author;
        authorLink.href = 'Author.html?author=' + encodeURIComponent(recipe.Author);
        authorLink.onclick = function() {
            handleAuthorClick(recipe.Author);
        };
        authorCell.appendChild(authorLink);
        row.appendChild(authorCell);

        const ingredientsCell = document.createElement('td');
        ingredientsCell.textContent = recipe.NumberOfIngredients;
        row.appendChild(ingredientsCell);

        const skillLevelCell = document.createElement('td');
        skillLevelCell.textContent = recipe.SkillLevel;
        row.appendChild(skillLevelCell);

        tableBody.appendChild(row);
    });
}

// Function to handle filter button click event
async function filterByIngredients() {
    const filterInput = document.getElementById('filterBy');
    const ingredient = filterInput.value.trim();

    // const ingredients = { ingredients: [ingredient] };

    const pageNumber = 1;
    const pageSize = 20;

    try {
        const recipesData = await fetchRecipes(pageNumber, pageSize, [ingredient]);
        if (recipesData) {
            const recipes = recipesData.recipes;
            updateRecipeTable(recipes);
        }
    } catch (error) {
        console.error('Error:', error);
    }
}

// Function to fetch recipes from the backend API
async function fetchRecipes(pageNumber = 1, pageSize = 20, ingredients = {}) {
    const url = 'https://localhost:7012/api/Recipes/All';
    try {
        const requestBody = {
            pageNumber: pageNumber,
            pageSize: pageSize,
            ingredients: ingredients
        };

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        if (response.ok) {
            const data = await response.json();
            return data;
        } else {
            console.error('Failed to fetch recipes:', response.statusText);
            return null;
        }
    } catch (error) {
        console.error('Error fetching recipes:', error);
        return null;
    }
}

// Fetch recipes and update filter input on page load
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
        const topElementsTable = document.getElementById('topElementsTable');
        const topElementsBody = document.getElementById('topElementsBody');
        topElementsBody.innerHTML = ''; 

        topElements.ingredients.forEach((ingredient, index) => {
            const row = topElementsBody.insertRow();
            const rankCell = row.insertCell(0);
            rankCell.textContent = index + 1;
            const ingredientsCell = row.insertCell(1);
            ingredientsCell.textContent = ingredient.name;
            const recipeCountCell = row.insertCell(2);
            recipeCountCell.textContent = ingredient.recipeCount;
        });

        topElements.authors.forEach((author, index) => {
            const row = topElementsBody.rows[index];
            if (!row) {
                row = topElementsBody.insertRow();
            }
            const authorsCell = row.insertCell(3);
            authorsCell.textContent = author.name;
            const recipeCountCell = row.insertCell(4);
            recipeCountCell.textContent = author.recipeCount;
        });


        topElements.recipes.forEach((recipe, index) => {
            const row = topElementsBody.rows[index];
            if (!row) {
                row = topElementsBody.insertRow();
            }
            const recipesCell = row.insertCell(5);
            recipesCell.textContent = recipe.name;
        });
    }
};

