const fs = require('fs');

const filePath = 'C:/Users/joeder-blanca/Documents/projetos-joe/git/nexas/frontend/portal-pon/src/app/features/modules/courses/pages/courses/courses.component.html';
let content = fs.readFileSync(filePath, 'utf8');

content = content.replace(
  '<div class="modal-dialog modal-lg">',
  '<div class="modal-dialog modal-xl modal-dialog-centered">'
);

content = content.replace(
  '<div class="modal-content rounded-4 border-0 shadow">',
  '<div class="modal-content rounded-4 border-0 shadow-lg d-flex flex-column" style="height: 90vh;">'
);

content = content.replace(
  '<form [formGroup]="courseForm" (ngSubmit)="saveCourse()">',
  '<form [formGroup]="courseForm" (ngSubmit)="saveCourse()" class="d-flex flex-column overflow-hidden flex-grow-1">'
);

content = content.replace(
  '<div class="modal-body bg-secondary bg-opacity-10 p-4">',
  '<div class="modal-body bg-secondary bg-opacity-10 p-4 overflow-auto custom-scrollbar flex-grow-1">'
);

fs.writeFileSync(filePath, content, 'utf8');
console.log('Fixed courseModal layout');
