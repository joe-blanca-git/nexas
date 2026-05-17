// Configuração do endpoint da API (Altere para localhost se estiver desenvolvendo localmente)
const API_BASE = "https://joederblanca.com.br/nexas-api/Courses";

document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("container-cursos")) {
        fetchCursos();
    }
    if (document.getElementById("course-name") || document.getElementById("curso-hero")) {
        fetchCursoDetalhes();
    }
});

function hidePreloader() {
    const preloader = document.getElementById("preloader");
    if (preloader) {
        setTimeout(() => {
            preloader.classList.add("loaded");
        }, 500);
    }
}

async function fetchCursos() {
    const container = document.getElementById("container-cursos");

    try {
        const response = await fetch(API_BASE);
        if (!response.ok) throw new Error("Erro ao buscar dados");
        
        const cursos = await response.json();
        container.innerHTML = "";

        cursos.forEach((curso, index) => {
            const col = document.createElement("div");
            col.className = `col-lg-4 col-md-6 reveal ${index > 0 ? 'delay-' + index : ''}`;
            
            const precoFormatado = curso.priceSingle ? curso.priceSingle.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
            }) : "R$ 0,00";

            col.innerHTML = `
                <div class="card course-card h-100">
                    <img src="${curso.imgCoverLink}" class="card-img-top" alt="${curso.name}" onerror="this.src='./assets/images/others/default.jpeg'">
                    <div class="card-body d-flex flex-column justify-content-between h-100">
                        <div>
                            <span class="course-tag">${curso.level || 'Intermediário'}</span>
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
        console.error("Erro ao buscar cursos:", error);
        container.innerHTML = `<p class="text-center py-5 text-muted">Não foi possível carregar os cursos. Tente novamente mais tarde.</p>`;
    } finally {
        hidePreloader();
    }
}

async function fetchCursoDetalhes() {
    const urlParams = new URLSearchParams(window.location.search);
    let id = urlParams.get("id");

    // Fallback to course 2 (Mestres do Operations Center) if no specific ID is provided
    if (!id) {
        console.warn("ID do curso não fornecido. Utilizando ID padrão: 2");
        id = "2";
    }

    try {
        const API_URL = `${API_BASE}/${id}`;
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Erro ao buscar detalhes do curso");

        const curso = await response.json();

        // 1. Atualizar informações básicas do curso
        const nameEl = document.getElementById("course-name");
        if (nameEl) nameEl.textContent = curso.name;

        const hero = document.getElementById("curso-hero");
        if (hero && curso.imgCoverLink) {
            hero.style.backgroundImage = `linear-gradient(rgba(15, 18, 16, 0.8), rgba(15, 18, 16, 0.9)), url('${curso.imgCoverLink}')`;
        }

        const coverImg = document.getElementById("course-cover-img");
        if (coverImg && curso.imgCoverLink) {
            coverImg.src = curso.imgCoverLink;
            coverImg.alt = curso.name;
            coverImg.style.display = "block";
        }

        const descSubEl = document.getElementById("course-description-sub");
        if (descSubEl) descSubEl.textContent = curso.descriptionSub || "";

        const levelEl = document.getElementById("course-level");
        if (levelEl) levelEl.textContent = curso.level || "Intermediário";

        const descEl = document.getElementById("course-description");
        if (descEl) descEl.textContent = curso.description || "";

        // Ocultar segundo parágrafo de descrição se não houver mais conteúdo
        const descSub2El = document.getElementById("course-description-sub2");
        if (descSub2El) descSub2El.style.display = "none";

        // 2. Calcular número total de aulas e duração
        let totalLessons = 0;
        let totalSeconds = 0;

        if (curso.modules) {
            curso.modules.forEach(m => {
                if (m.lessons) {
                    totalLessons += m.lessons.length;
                    m.lessons.forEach(l => {
                        totalSeconds += (l.durationSeconds || 0);
                    });
                }
            });
        }

        const lessonsCountEl = document.getElementById("course-lessons-count");
        if (lessonsCountEl) lessonsCountEl.textContent = `${totalLessons} Vídeos`;

        const includeLessonsEl = document.getElementById("include-lessons-count");
        if (includeLessonsEl) includeLessonsEl.textContent = `${totalLessons} aulas gravadas`;

        const durationEl = document.getElementById("course-duration");
        if (durationEl) {
            const totalHours = Math.round(totalSeconds / 3600);
            durationEl.textContent = totalHours > 0 ? `${totalHours} Horas` : `${Math.round(totalSeconds / 60)} Minutos`;
        }

        // 3. Atualizar preços e investimento
        const priceValEl = document.getElementById("price-value");
        if (priceValEl && curso.priceSingle) {
            priceValEl.textContent = curso.priceSingle.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
            });
        }

        const priceInstallmentsEl = document.getElementById("price-installments");
        if (priceInstallmentsEl && curso.priceSingle) {
            const installmentVal = (curso.priceSingle / 12).toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
            });
            priceInstallmentsEl.textContent = `ou 12x de ${installmentVal}`;
        }

        const priceCourseNameEl = document.getElementById("price-course-name");
        if (priceCourseNameEl) priceCourseNameEl.textContent = curso.name;

        // 4. Renderizar a grade curricular (Modules -> Lessons)
        const accordion = document.getElementById("curriculumAccordion");
        if (accordion && curso.modules) {
            accordion.innerHTML = "";
            curso.modules.forEach((mod, modIdx) => {
                const accordionItem = document.createElement("div");
                accordionItem.className = "accordion-item mb-3 shadow-sm border-0";
                
                const isShow = modIdx === 0 ? "show" : "";
                const isCollapsed = modIdx === 0 ? "" : "collapsed";
                const isExpanded = modIdx === 0 ? "true" : "false";
                
                let lessonsListHtml = "";
                if (mod.lessons && mod.lessons.length > 0) {
                    mod.lessons.forEach(lesson => {
                        const durationMin = lesson.durationSeconds ? `${Math.round(lesson.durationSeconds / 60)} min` : "";
                        lessonsListHtml += `
                            <li class="list-group-item d-flex justify-content-between align-items-center py-3 bg-transparent">
                                <div class="d-flex align-items-center gap-3">
                                    <i class="bi bi-play-circle text-primary"></i> ${lesson.name}
                                </div>
                                <span class="badge bg-light text-muted fw-normal">${durationMin}</span>
                            </li>
                        `;
                    });
                } else {
                    lessonsListHtml = `<li class="list-group-item py-3 bg-transparent text-muted small">Nenhuma aula cadastrada para este módulo ainda.</li>`;
                }
                
                accordionItem.innerHTML = `
                    <h2 class="accordion-header">
                        <button class="accordion-button fw-bold ${isCollapsed}" type="button" data-bs-toggle="collapse" data-bs-target="#mod-${mod.id}" aria-expanded="${isExpanded}">
                            ${mod.name}
                        </button>
                    </h2>
                    <div id="mod-${mod.id}" class="accordion-collapse collapse ${isShow}" data-bs-parent="#curriculumAccordion">
                        <div class="accordion-body p-0">
                            <ul class="list-group list-group-flush">
                                ${lessonsListHtml}
                            </ul>
                        </div>
                    </div>
                `;
                accordion.appendChild(accordionItem);
            });
        }

    } catch (error) {
        console.error("Erro ao carregar detalhes do curso:", error);
        const nameEl = document.getElementById("course-name");
        if (nameEl) nameEl.textContent = "Erro ao carregar curso";
        
        const accordion = document.getElementById("curriculumAccordion");
        if (accordion) {
            accordion.innerHTML = `<div class="p-4 text-center text-danger">Não foi possível carregar a grade curricular. Verifique se o backend está rodando.</div>`;
        }
    } finally {
        hidePreloader();
    }
}