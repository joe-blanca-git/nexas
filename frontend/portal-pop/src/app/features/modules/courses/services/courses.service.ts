import { Injectable, Injector } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';
import { Course } from '../models/course.model';

import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class CoursesService extends BaseService {

  constructor(protected override injector: Injector, private http: HttpClient) {
    super(injector);
  }

  getCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.urlApiNexas}courses`, this.GetAuthHeaderJson());
  }

  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.urlApiNexas}course-categories`, this.GetAuthHeaderJson());
  }

  createCourse(courseData: Partial<Course>): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses`, courseData, this.GetAuthHeaderJson());
  }

  toggleCourseStatus(id: number): Observable<{ active: boolean }> {
    return this.http.patch<{ active: boolean }>(`${this.urlApiNexas}courses/${id}/toggle-status`, {}, this.GetAuthHeaderJson());
  }

  createModule(moduleData: any): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses/modules`, moduleData, this.GetAuthHeaderJson());
  }

  createLesson(lessonData: any): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses/lessons`, lessonData, this.GetAuthHeaderJson());
  }

  uploadImage(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.urlApiNexas}uploads/image`, formData, this.GetAuthHeaderUploadJson());
  }

  generateVideoUpload(lessonId: number): Observable<any> {
    return this.http.post<any>(`${this.urlApiNexas}courses/lessons/${lessonId}/video`, {}, this.GetAuthHeaderJson());
  }

  completeVideoUpload(lessonId: number): Observable<any> {
    return this.http.post<any>(`${this.urlApiNexas}courses/lessons/${lessonId}/video/complete`, {}, this.GetAuthHeaderJson());
  }
}

const COURSES_MOCK: Course[] = [

];
