const fs = require('fs');
const path = require('path');

const filesToProcess = [
  'C:/Users/joeder-blanca/Documents/projetos-joe/git/nexas/frontend/portal-pon/src/app/features/modules/courses/pages/courses/courses.component.html',
  'C:/Users/joeder-blanca/Documents/projetos-joe/git/nexas/frontend/portal-pon/src/app/features/modules/courses/components/lesson-modal/lesson-modal.component.html',
  'C:/Users/joeder-blanca/Documents/projetos-joe/git/nexas/frontend/portal-pon/src/app/features/modules/courses/components/module-modal/module-modal.component.html',
  'C:/Users/joeder-blanca/Documents/projetos-joe/git/nexas/frontend/portal-pon/src/app/features/modules/blog/pages/blog-list/blog-list.component.html'
];

function getModalTheme(id, titleText) {
  const text = (id + ' ' + titleText).toLowerCase();
  
  if (text.includes('teacher') || text.includes('professor')) {
    return { icon: 'fa-chalkboard-teacher', color: 'primary' };
  }
  if (text.includes('lesson') || text.includes('aula')) {
    return { icon: 'fa-play', color: 'danger' };
  }
  if (text.includes('module') || text.includes('módulo')) {
    return { icon: 'fa-layer-group', color: 'primary' };
  }
  if (text.includes('course') || text.includes('curso')) {
    return { icon: 'fa-graduation-cap', color: 'success' };
  }
  if (text.includes('domain') || text.includes('domínio')) {
    return { icon: 'fa-globe', color: 'info' };
  }
  if (text.includes('blog') || text.includes('publicação')) {
    return { icon: 'fa-newspaper', color: 'primary' };
  }
  if (text.includes('delete') || text.includes('excluir') || text.includes('confirma') || text.includes('toggle')) {
    return { icon: 'fa-exclamation-triangle', color: 'warning' };
  }
  
  return { icon: 'fa-star', color: 'secondary' };
}

function processFile(filePath) {
  if (!fs.existsSync(filePath)) return;
  
  let content = fs.readFileSync(filePath, 'utf8');
  let originalContent = content;

  const regex = /<(div)[^>]*?(?:modal-header|card-header)[^>]*>([\s\S]*?)<\/\1>/g;
  
  content = content.replace(regex, (match, tag, innerContent) => {
    const titleMatch = innerContent.match(/<h5[^>]*>(.*?)<\/h5>/);
    let title = titleMatch ? titleMatch[1] : '';
    title = title.replace(/<i[^>]*><\/i>/, '').trim(); 
    title = title.replace(/<i[^>]*>.*?<\/i>/, '').trim();
    
    const subMatch = innerContent.match(/<(?:small|p)[^>]*>(.*?)<\/(?:small|p)>/);
    let sub = subMatch ? subMatch[1] : '';
    
    if (!title) return match; 
    if (title.includes('Confirmação') || title.includes('Excluir') || title.includes('Tem certeza')) {
       // Keep delete/confirm modals simple as they were or standard? User said "todos".
       // We'll apply the theme but use warning styles.
    }
    
    const theme = getModalTheme('', title);
    
    let closeAction = 'data-bs-dismiss="modal"';
    if (match.includes('closeModal()')) closeAction = '(click)="closeModal()"';
    else if (match.includes('cancel()')) closeAction = '(click)="cancel()"';
    else if (match.includes('closeTeacherListModal')) closeAction = '(click)="closeTeacherListModal()"';
    else if (match.includes('closeLessonListModal')) closeAction = '(click)="closeLessonListModal()"';
    else if (match.includes('closeModuleListModal')) closeAction = '(click)="closeModuleListModal()"';
    
    let disabledAttr = match.includes('isSavingLesson') ? ' [disabled]="isSavingLesson"' : '';

    let newHeader = `
      <!-- Premium Header -->
      <div class="${tag === 'div' ? (match.includes('card-header') ? 'card-header' : 'modal-header') : tag} bg-dark border-bottom-0 pb-3 pt-4 px-4 rounded-top-4 position-relative overflow-hidden flex-shrink-0">
        <div class="position-absolute top-0 end-0 p-3 opacity-25">
          <i class="fas ${theme.icon} text-white" style="font-size: 8rem; transform: translate(20%, -30%);"></i>
        </div>
        <div class="d-flex align-items-center gap-3">
          <div class="bg-${theme.color} bg-opacity-10 text-${theme.color} rounded-circle d-flex align-items-center justify-content-center shadow-sm flex-shrink-0" style="width: 48px; height: 48px;">
            <i class="fas ${theme.icon} fs-5"></i>
          </div>
          <div>
            <h5 class="modal-title fw-bolder text-white mb-0 fs-5">${title}</h5>
            ${sub ? `<small class="text-white-50 fw-medium d-block">${sub}</small>` : ''}
          </div>
        </div>
        <button type="button" class="btn-close position-absolute top-0 end-0 mt-4 me-4" ${closeAction} aria-label="Close"${disabledAttr}></button>
      </div>`;
      
      return newHeader.trim();
  });
  
  if (content !== originalContent) {
    fs.writeFileSync(filePath, content, 'utf8');
    console.log('Updated: ' + filePath);
  }
}

filesToProcess.forEach(processFile);
console.log('Done');
