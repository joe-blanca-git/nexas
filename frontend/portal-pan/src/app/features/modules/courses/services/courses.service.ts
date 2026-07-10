import { HttpClient } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';

@Injectable({
  providedIn: 'root'
})
export class CoursesService extends BaseService {
  constructor(injector: Injector, private httpClient: HttpClient) {
    super(injector);
  }

  async getMyCourses(): Promise<any[]> {
    try {
      let url = `${this.urlApiNexas}portal/courses`;
      const response = await firstValueFrom(
        this.httpClient.get<any[]>(url, this.GetAuthHeaderJson())
      );
      return this.extractData(response) as any[];
    } catch (error) {
      throw error;
    }
  }

  async getCourseDetail(id: number): Promise<any> {
    try {
      let url = `${this.urlApiNexas}portal/courses/course-detail/${id}`;
      const response = await firstValueFrom(
        this.httpClient.get<any>(url, this.GetAuthHeaderJson())
      );
      return this.extractData(response);
    } catch (error) {
      throw error;
    }
  }

  async getCourseCheckoutSummary(id: number): Promise<any> {
    try {
      let url = `${this.urlApiNexas}portal/courses/${id}/checkout-summary`;
      const response = await firstValueFrom(
        this.httpClient.get<any>(url, this.GetAuthHeaderJson())
      );
      return this.extractData(response);
    } catch (error) {
      throw error;
    }
  }

  async toggleLessonView(lessonId: number): Promise<any> {
    try {
      let url = `${this.urlApiNexas}lessons/${lessonId}/toggle-view`;
      const response = await firstValueFrom(
        this.httpClient.post<any>(url, {}, this.GetAuthHeaderJson())
      );
      return this.extractData(response);
    } catch (error) {
      throw error;
    }
  }

  async rateCourse(courseId: number, rate: number): Promise<any> {
    try {
      let url = `${this.urlApiNexas}portal/courses/${courseId}/rate`;
      const body = { rate: rate };
      const response = await firstValueFrom(
        this.httpClient.post<any>(url, body, this.GetAuthHeaderJson())
      );
      return this.extractData(response);
    } catch (error) {
      throw error;
    }
  }
}
