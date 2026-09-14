import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { BaseService } from '../../../../core/services/base.service';

@Injectable({
  providedIn: 'root'
})
export class SettingsService extends BaseService {

  constructor(protected override injector: Injector, private http: HttpClient) {
    super(injector);
  }

  // --- Course Categories ---

  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.urlApiNexas}course-categories`, this.GetAuthHeaderJson());
  }

  getCategoryById(id: number): Observable<any> {
    return this.http.get<any>(`${this.urlApiNexas}course-categories/${id}`, this.GetAuthHeaderJson());
  }

  createCategory(categoryData: { name: string, description?: string }): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}course-categories`, categoryData, this.GetAuthHeaderJson());
  }

  updateCategory(id: number, categoryData: { id: number, name: string, description?: string }): Observable<any> {
    return this.http.put<any>(`${this.urlApiNexas}course-categories/${id}`, categoryData, this.GetAuthHeaderJson());
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete<any>(`${this.urlApiNexas}course-categories/${id}`, this.GetAuthHeaderJson());
  }

  // --- Forum Categories ---

  getForumCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.urlApiNexas}ForumCategories`, this.GetAuthHeaderJson());
  }

  getForumCategoryById(id: number): Observable<any> {
    return this.http.get<any>(`${this.urlApiNexas}ForumCategories/${id}`, this.GetAuthHeaderJson());
  }

  createForumCategory(categoryData: { name: string, description?: string, icon?: string }): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}ForumCategories`, categoryData, this.GetAuthHeaderJson());
  }

  updateForumCategory(id: number, categoryData: { id: number, name: string, description?: string, active?: boolean, icon?: string }): Observable<any> {
    return this.http.put<any>(`${this.urlApiNexas}ForumCategories/${id}`, categoryData, this.GetAuthHeaderJson());
  }

  deleteForumCategory(id: number): Observable<any> {
    return this.http.delete<any>(`${this.urlApiNexas}ForumCategories/${id}`, this.GetAuthHeaderJson());
  }

  // --- Teachers ---

  getTeachers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.urlApiNexas}Teachers`, this.GetAuthHeaderJson());
  }

  getTeacherById(id: number): Observable<any> {
    return this.http.get<any>(`${this.urlApiNexas}Teachers/${id}`, this.GetAuthHeaderJson());
  }

  createTeacher(teacherData: any): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}Teachers`, teacherData, this.GetAuthHeaderJson());
  }

  updateTeacher(id: number, teacherData: any): Observable<any> {
    return this.http.put<any>(`${this.urlApiNexas}Teachers/${id}`, teacherData, this.GetAuthHeaderJson());
  }

  deleteTeacher(id: number): Observable<any> {
    return this.http.delete<any>(`${this.urlApiNexas}Teachers/${id}`, this.GetAuthHeaderJson());
  }

  getSystemUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.urlApiNexas}Users`, this.GetAuthHeaderJson());
  }
}
