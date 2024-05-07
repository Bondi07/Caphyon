
/* AFISAREA RECETELOR */
let http = new XMLHttpRequest();

let data = {
    "pageNumber": 1,
    "pageSize": 20
};

let jsonData = JSON.stringify(data);

http.open('POST', 'https://localhost:7012/api/Recipes/All', true);
http.setRequestHeader('Content-Type', 'application/json');
http.send(jsonData); 

http.onload = function(){

    if(this.readyState == 4 && this.status == 200){

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
        
        for(let item of recipes){
            out += `
                <tr>
                    <td>${item.name}</td>
                    <td>${item.author}</td>
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




/* PAGIANTION */
