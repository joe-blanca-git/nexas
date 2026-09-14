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

  updateCourse(courseData: Partial<Course>): Observable<any> {
    return this.http.put<any>(`${this.urlApiNexas}courses`, courseData, this.GetAuthHeaderJson());
  }

  toggleCourseStatus(id: number): Observable<{ active: boolean }> {
    return this.http.patch<{ active: boolean }>(`${this.urlApiNexas}courses/${id}/toggle-status`, {}, this.GetAuthHeaderJson());
  }

  createModule(moduleData: any): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses/modules`, moduleData, this.GetAuthHeaderJson());
  }

  updateModule(moduleData: any): Observable<any> {
    return this.http.put<any>(`${this.urlApiNexas}courses/modules`, moduleData, this.GetAuthHeaderJson());
  }

  deleteModule(id: number): Observable<any> {
    return this.http.delete<any>(`${this.urlApiNexas}courses/modules/${id}`, this.GetAuthHeaderJson());
  }

  createLesson(lessonData: any): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses/lessons`, lessonData, this.GetAuthHeaderJson());
  }

  updateLesson(lessonData: any): Observable<any> {
    return this.http.put<any>(`${this.urlApiNexas}courses/lessons`, lessonData, this.GetAuthHeaderJson());
  }

  deleteLesson(id: number): Observable<any> {
    return this.http.delete<any>(`${this.urlApiNexas}courses/lessons/${id}`, this.GetAuthHeaderJson());
  }

  createDomain(courseId: number, domainData: any): Observable<any> {
    return this.http.post<any>(`${this.urlApiNexas}Courses/${courseId}/domains`, domainData, this.GetAuthHeaderJson());
  }

  updateDomain(courseId: number, domainData: any): Observable<any> {
    return this.http.put<any>(`${this.urlApiNexas}Courses/${courseId}/domains/${domainData.id}`, domainData, this.GetAuthHeaderJson());
  }

  deleteDomain(courseId: number, domainId: number): Observable<any> {
    return this.http.delete<any>(`${this.urlApiNexas}Courses/${courseId}/domains/${domainId}`, this.GetAuthHeaderJson());
  }

  getTeachers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.urlApiNexas}Teachers`, this.GetAuthHeaderJson());
  }

  assignTeacher(teacherId: number, courseId: number): Observable<any> {
    return this.http.post<any>(`${this.urlApiNexas}Teachers/assign`, { teacherId, courseId }, this.GetAuthHeaderJson());
  }

  unassignTeacher(teacherId: number, courseId: number): Observable<any> {
    return this.http.post<any>(`${this.urlApiNexas}Teachers/unassign`, { teacherId, courseId }, this.GetAuthHeaderJson());
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
