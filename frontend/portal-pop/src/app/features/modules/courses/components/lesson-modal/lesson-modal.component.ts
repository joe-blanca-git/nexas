import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Lesson } from '../../models/course.model';

@Component({
  selector: 'app-lesson-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './lesson-modal.component.html',
  styleUrl: './lesson-modal.component.scss'
})
export class LessonModalComponent implements OnInit {
  @Input() lessonData!: Lesson;
  @Output() closeModal = new EventEmitter<void>();
  @Output() saveLessonEvent = new EventEmitter<Lesson>();

  lessonForm!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
    this.lessonForm = this.fb.group({
      name: [this.lessonData.name, Validators.required],
      description: [this.lessonData.description],
      durationSeconds: [this.lessonData.durationSeconds, [Validators.min(0)]],
      bunnyVideoId: [this.lessonData.bunnyVideoId]
    });
  }

  get f() { return this.lessonForm.controls; }

  save() {
    if (this.lessonForm.invalid) {
      this.lessonForm.markAllAsTouched();
      return;
    }
    
    const updatedLesson: Lesson = {
      ...this.lessonData,
      ...this.lessonForm.value
    };

    this.saveLessonEvent.emit(updatedLesson);
  }

  cancel() {
    this.closeModal.emit();
  }
}
