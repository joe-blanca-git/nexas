import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Module, Lesson } from '../../models/course.model';
import { LessonModalComponent } from '../lesson-modal/lesson-modal.component';

@Component({
  selector: 'app-module-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LessonModalComponent],
  templateUrl: './module-modal.component.html',
  styleUrl: './module-modal.component.scss'
})
export class ModuleModalComponent implements OnInit {
  @Input() moduleData!: Module;
  @Output() closeModal = new EventEmitter<void>();
  @Output() saveModuleEvent = new EventEmitter<Module>();

  moduleForm!: FormGroup;
  lessons: Lesson[] = [];

  // Lesson Modal State
  isLessonModalOpen = false;
  selectedLesson: Lesson | null = null;
  editingLessonIndex: number = -1;

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
    this.moduleForm = this.fb.group({
      name: [this.moduleData.name, Validators.required],
      description: [this.moduleData.description],
      descriptionSub: [this.moduleData.descriptionSub],
      imgCoverLink: [this.moduleData.imgCoverLink]
    });
    // Deep copy lessons so cancel discards changes
    this.lessons = JSON.parse(JSON.stringify(this.moduleData.lessons || []));
  }

  get f() { return this.moduleForm.controls; }

  save() {
    if (this.moduleForm.invalid) {
      this.moduleForm.markAllAsTouched();
      return;
    }
    
    const updatedModule: Module = {
      ...this.moduleData,
      ...this.moduleForm.value,
      lessons: this.lessons
    };

    this.saveModuleEvent.emit(updatedModule);
  }

  cancel() {
    this.closeModal.emit();
  }

  // --- Lesson Management ---

  openLessonModal(lessonToEdit?: Lesson, index?: number) {
    if (lessonToEdit && index !== undefined) {
      this.selectedLesson = JSON.parse(JSON.stringify(lessonToEdit));
      this.editingLessonIndex = index;
    } else {
      this.selectedLesson = {
        id: Date.now(),
        name: '',
        description: '',
        durationSeconds: 0,
        bunnyVideoId: ''
      };
      this.editingLessonIndex = -1;
    }
    this.isLessonModalOpen = true;
  }

  closeLessonModal() {
    this.isLessonModalOpen = false;
    this.selectedLesson = null;
    this.editingLessonIndex = -1;
  }

  saveLesson(lessonData: Lesson) {
    if (this.editingLessonIndex > -1) {
      this.lessons[this.editingLessonIndex] = lessonData;
    } else {
      this.lessons.push(lessonData);
    }
    this.closeLessonModal();
  }

  removeLesson(index: number) {
    if (confirm('Tem certeza que deseja excluir esta aula?')) {
      this.lessons.splice(index, 1);
    }
  }

  moveLessonUp(index: number) {
    if (index > 0) {
      const temp = this.lessons[index];
      this.lessons[index] = this.lessons[index - 1];
      this.lessons[index - 1] = temp;
    }
  }

  moveLessonDown(index: number) {
    if (index < this.lessons.length - 1) {
      const temp = this.lessons[index];
      this.lessons[index] = this.lessons[index + 1];
      this.lessons[index + 1] = temp;
    }
  }
}
