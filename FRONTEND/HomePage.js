
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

/* PAGIANTION */

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

// Function to disable or enable the previous page button and update its style
function updatePrevButton() {
    const prevPageButton = document.getElementById('prevPage');
    if (currentPage === 1) {
        prevPageButton.disabled = true;
        prevPageButton.style.backgroundColor = 'lightgrey'; // Change the background color
        prevPageButton.style.cursor = 'not-allowed'; // Change cursor to indicate it's not clickable
    } else {
        prevPageButton.disabled = false;
        prevPageButton.style.backgroundColor = '#4CAF50'; // Reset background color
        prevPageButton.style.cursor = 'pointer'; // Reset cursor
    }
}

document.getElementById('prevPage').addEventListener('click', () => {
    if (currentPage > 1) {
        currentPage--;
        fetchAndRenderRecipes(currentPage);
        updatePrevButton(); // Update the style after changing the page
    }
});

document.getElementById('nextPage').addEventListener('click', () => {
    currentPage++;
    fetchAndRenderRecipes(currentPage);
    updatePrevButton(); // Update the style after changing the page
});

// Fetch and render recipes for the initial page load
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

        // Check if 'recipeBody' element exists before updating its innerHTML
        const recipeBody = document.getElementById('recipeBody');
        if (recipeBody) {
            recipeBody.innerHTML = ''; // Clear previous search results

            recipes.forEach(recipe => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td><a href="RecipeDetails.html?id=${recipe.Id}">${recipe.Name}</a></td>
                    <td>${recipe.Author}</td>
                    <td>${recipe.NumberOfIngredients}</td>
                    <td>${recipe.SkillLevel}</td>
                `;
                recipeBody.appendChild(row);
            });
        } else {
            console.error("Element with ID 'recipeBody' not found.");
        }
    }

    document.getElementById('searchButton').addEventListener('click', searchSimilarRecipes);

});



/* AUTHOR COLUM PAGE  */


// Function to handle click on author's name
function handleAuthorClick(authorName) {
    // Redirect to Author.html page with author's name as query parameter
    window.location.href = `Author.html?author=${encodeURIComponent(authorName)}`;
}





