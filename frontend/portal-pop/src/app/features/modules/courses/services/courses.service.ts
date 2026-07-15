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

  createCourse(courseData: Partial<Course>): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses`, courseData, this.GetAuthHeaderJson());
  }

  createModule(moduleData: any): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses/modules`, moduleData, this.GetAuthHeaderJson());
  }

  createLesson(lessonData: any): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}courses/lessons`, lessonData, this.GetAuthHeaderJson());
  }
}

const COURSES_MOCK: Course[] = [

];
