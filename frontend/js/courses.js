document.addEventListener("DOMContentLoaded", () => {
    fetchCursos();
});

async function fetchCursos() {
    const container = document.getElementById("container-cursos");
    const preloader = document.getElementById("preloader");
    const API_URL = "https://joederblanca.com.br/nexas-api/Courses"; 

    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Erro ao buscar dados");
        
        const cursos = await response.json();
        container.innerHTML = "";

        cursos.forEach((curso, index) => {
            const col = document.createElement("div");
            col.className = `col-lg-4 col-md-6 reveal ${index > 0 ? 'delay-' + index : ''}`;
            
            const precoFormatado = curso.priceSingle.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
            });

            col.innerHTML = `
                <div class="card course-card h-100">
                    <img src="${curso.imgCoverLink}" class="card-img-top" alt="${curso.name}" onerror="this.src='./assets/images/others/default.jpeg'">
                    <div class="card-body d-flex flex-column justify-content-between h-100">
                        <div>
                            <span class="course-tag">${curso.level}</span>
                            <h4 class="card-title">${curso.name}</h4>
                            <p class="card-text text-muted small">${curso.description}</p>
                        </div>
                        <div>
                            <hr class="my-4 opacity-10">
                            <div class="d-flex justify-content-between align-items-center flex-column flex-md-row">
                                <span class="fw-bold text-primary fs-5">${precoFormatado}</span>
                                <a href="curso-detalhes.html?id=${curso.id}" class="btn btn-primary btn-sm">Ver detalhes</a>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            container.appendChild(col);
        });

    } catch (error) {
        console.error("Erro:", error);
        container.innerHTML = `<p class="text-center py-5">Não foi possível carregar os cursos. Tente novamente mais tarde.</p>`;
    } finally {
        // ESSA É A PARTE CHAVE: 
        // Adiciona um pequeno delay de 500ms para a transição não ser brusca demais
        setTimeout(() => {
            preloader.classList.add("loaded");
        }, 500);
    }
}