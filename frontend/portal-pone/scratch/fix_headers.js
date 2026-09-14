const fs = require('fs');

const file = 'C:/Users/joeder-blanca/Documents/projetos-joe/git/nexas/frontend/portal-pon/src/app/features/modules/courses/pages/courses/courses.component.html';
let content = fs.readFileSync(file, 'utf8');

const modals = [
  { id: 'moduleListModal', icon: 'fa-layer-group', color: 'primary', title: 'Módulos do Curso', sub: '{{ selectedCourseForModules?.name }}', close: '(click)="closeModuleListModal()"' },
  { id: 'moduleModal', icon: 'fa-layer-group', color: 'primary', title: `{{ moduleToEdit ? 'Editar Módulo' : 'Novo Módulo' }}`, sub: `{{ moduleToEdit ? 'Altere as informações do módulo' : 'Adicione uma nova seção ao curso' }}`, close: 'data-bs-dismiss="modal"' },
  { id: 'lessonListModal', icon: 'fa-play-circle', color: 'danger', title: 'Aulas do Curso', sub: '{{ selectedCourseForLessons?.name }}', close: '(click)="closeLessonListModal()"' },
  { id: 'lessonModal', icon: 'fa-play-circle', color: 'danger', title: `{{ lessonToEdit ? 'Editar Aula' : 'Nova Aula' }}`, sub: `{{ lessonToEdit ? 'Altere as informações da aula' : 'Adicione uma aula a um módulo existente' }}`, close: 'data-bs-dismiss="modal" [disabled]="isSavingLesson"' },
  { id: 'domainListModal', icon: 'fa-globe', color: 'info', title: 'Domínios / Benefícios', sub: '{{ selectedCourseForDomains?.name }}', close: '(click)="closeDomainListModal()"' },
  { id: 'domainModal', icon: 'fa-globe', color: 'info', title: `{{ domainToEdit ? 'Editar Domínio' : 'Novo Domínio' }}`, sub: `{{ domainToEdit ? 'Altere as informações do domínio' : 'Adicione um novo domínio/benefício ao curso' }}`, close: 'data-bs-dismiss="modal"' }
];

for (const modal of modals) {
  const searchString = `id="${modal.id}"`;
  const startIndex = content.indexOf(searchString);
  if (startIndex === -1) {
    console.log(`Could not find ${modal.id}`);
    continue;
  }
  
  const headerStart = content.indexOf('<div class="modal-header', startIndex);
  let headerEnd1 = content.indexOf('<div class="modal-body', headerStart);
  let headerEnd2 = content.indexOf('<form', headerStart);
  let headerEnd3 = content.indexOf('<div class="table-responsive', headerStart);
  
  let options = [headerEnd1, headerEnd2, headerEnd3].filter(x => x > headerStart);
  let headerEnd = Math.min(...options);
  
  if (headerEnd === Infinity) {
    console.log(`Could not find end of header for ${modal.id}`);
    continue;
  }
  
  const newHeader = `<div class="modal-header bg-dark border-bottom-0 pb-3 pt-4 px-4 rounded-top-4 position-relative overflow-hidden flex-shrink-0">
        <div class="position-absolute top-0 end-0 p-3 opacity-25">
          <i class="fas ${modal.icon} text-white" style="font-size: 8rem; transform: translate(20%, -30%);"></i>
        </div>
        <div class="d-flex align-items-center gap-3">
          <div class="bg-${modal.color} bg-opacity-10 text-${modal.color} rounded-circle d-flex align-items-center justify-content-center shadow-sm flex-shrink-0" style="width: 48px; height: 48px;">
            <i class="fas ${modal.icon} fs-5"></i>
          </div>
          <div>
            <h5 class="modal-title fw-bolder text-white mb-0 fs-5">${modal.title}</h5>
            <small class="text-white-50 fw-medium d-block" style="max-width: 300px;">${modal.sub}</small>
          </div>
        </div>
        <button type="button" class="btn-close position-absolute top-0 end-0 mt-4 me-4" ${modal.close} aria-label="Close"></button>
      </div>
      `;
      
  content = content.substring(0, headerStart) + newHeader + content.substring(headerEnd);
}

fs.writeFileSync(file, content, 'utf8');
console.log('Fixed modals manually.');
