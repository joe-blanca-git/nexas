// Configuração do endpoint da API (Altere para localhost se estiver desenvolvendo localmente)
const API_BASE = "https://joederblanca.com.br/nexas-api/v1/api/Courses";
const API_TEACHERS = "https://joederblanca.com.br/nexas-api/v1/api/Teachers";

document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("container-cursos")) {
        fetchCursos();
    }
    if (document.getElementById("teachers-container")) {
        fetchTeachers();
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
                            <p class="card-text text-muted small text-justify">${curso.description.length > 150 ? curso.description.substring(0, 150) + '...' : curso.description}</p>
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

async function fetchTeachers() {
    const container = document.getElementById("teachers-container");
    if (!container) return;

    try {
        const response = await fetch(API_TEACHERS);
        if (!response.ok) throw new Error("Erro ao buscar professores");
        
        const teachers = await response.json();
        container.innerHTML = "";

        teachers.forEach((teacher, index) => {
            const col = document.createElement("div");
            col.className = `col-lg-5 col-md-10 text-center reveal ${index > 0 ? 'delay-' + (index % 3) : ''}`;
            
            const avatar = teacher.avatar ? teacher.avatar : "https://joederblanca.com.br/assets/img/profile/default-avatar.png";
            
            // Format role and position separated by <br>
            let roleHtml = "";
            if (teacher.role && teacher.position) {
                roleHtml = `${teacher.role} <br> ${teacher.position}`;
            } else if (teacher.role) {
                roleHtml = teacher.role;
            } else if (teacher.position) {
                roleHtml = teacher.position;
            }

            let socialLinks = "";
            if (teacher.linkedinLink) {
                socialLinks += `<a href="${teacher.linkedinLink}" target="_blank" class="text-primary me-3 fs-5"><i class="fab fa-linkedin"></i></a>`;
            }
            if (teacher.instagramLink) {
                socialLinks += `<a href="${teacher.instagramLink}" target="_blank" class="text-primary fs-5"><i class="fab fa-instagram"></i></a>`;
            }

            col.innerHTML = `
                <div class="mb-4">
                    <img src="${avatar}" class="rounded-circle shadow-lg mb-3" style="width: 180px; height: 180px; object-fit: cover; border: 6px solid var(--bg-light);" alt="${teacher.name}">
                </div>
                <h4 class="fw-bold mb-1">${teacher.name}</h4>
                <p class="text-primary fw-bold mb-3">${roleHtml}</p>
                <p class="text-muted px-lg-5">${teacher.bio || ''}</p>
                <div class="mt-3">
                    ${socialLinks}
                </div>
            `;
            container.appendChild(col);
        });

    } catch (error) {
        console.error("Erro ao buscar professores:", error);
        container.innerHTML = `<p class="text-center py-5 text-muted">Não foi possível carregar o time de professores no momento.</p>`;
    }
}

async function fetchCursoDetalhes() {
    const urlParams = new URLSearchParams(window.location.search);
    let id = urlParams.get("id");

    // Redireciona para a home caso o ID não seja fornecido na URL
    if (!id) {
        window.location.href = "index.html#home";
        return;
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
            const installmentVal = (curso.priceSingle / 10).toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
            });
            priceInstallmentsEl.textContent = `ou 10x de ${installmentVal}`;
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

        // 5. Renderizar os domínios (O que você vai dominar)
        const domainsContainer = document.getElementById("course-domains-container");
        if (domainsContainer && curso.domains && curso.domains.length > 0) {
            domainsContainer.innerHTML = "";
            curso.domains.forEach(domain => {
                domainsContainer.innerHTML += `
                    <div class="col-md-6">
                        <div class="d-flex gap-3">
                            <i class="bi bi-check-circle-fill text-primary fs-4"></i>
                            <div>
                                <h6 class="fw-bold mb-1">${domain.title}</h6>
                                <p class="small text-muted">${domain.description}</p>
                            </div>
                        </div>
                    </div>
                `;
            });
        }

        // 6. Renderizar os professores
        const teachersContainer = document.getElementById("course-teachers-container");
        if (teachersContainer && curso.teachers && curso.teachers.length > 0) {
            teachersContainer.innerHTML = "";
            curso.teachers.forEach(teacher => {
                // Se Avatar for base64 ou URL usa ele, senao default
                const avatar = teacher.avatar ? teacher.avatar : "https://joederblanca.com.br/assets/img/profile/default-avatar.png";
                
                // Concatena Role e Position se os dois existirem
                const rolePosition = [teacher.role, teacher.position].filter(Boolean).join(" & ");
                
                let socialLinks = "";
                if (teacher.linkedinLink) {
                    socialLinks += `<a href="${teacher.linkedinLink}" target="_blank" class="text-primary me-3 fs-4"><i class="fab fa-linkedin"></i></a>`;
                }
                if (teacher.instagramLink) {
                    socialLinks += `<a href="${teacher.instagramLink}" target="_blank" class="text-primary fs-4"><i class="fab fa-instagram"></i></a>`;
                }

                teachersContainer.innerHTML += `
                    <div class="card p-5 border-0 bg-light-agro rounded-5 mb-4 shadow-sm reveal">
                        <div class="row align-items-center g-5">
                            <div class="col-md-4 text-center">
                                <img src="${avatar}" class="rounded-circle shadow-lg mb-3" style="width: 200px; height: 200px; object-fit: cover; border: 8px solid white;" alt="${teacher.name}">
                            </div>
                            <div class="col-md-8">
                                <span class="text-primary fw-bold text-uppercase d-block mb-2">Seu Professor</span>
                                <h3 class="fw-bold mb-3">${teacher.name}</h3>
                                <p class="text-primary fw-bold mb-4">${rolePosition}</p>
                                <p class="text-muted fs-5 text-justify">${teacher.bio || ''}</p>
                                <div class="mt-4">
                                    ${socialLinks}
                                </div>
                            </div>
                        </div>
                    </div>
                `;
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