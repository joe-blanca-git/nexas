import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';

export interface IForumCategoryDto {
  id: number;
  name: string;
  description: string;
  active: boolean;
}

export interface IForumTopicSummaryDto {
  id: number;
  title: string;
  status: string; // 'Open', 'Resolved'
  categoryName: string;
  authorName: string;
  repliesCount: number;
  createdAt: string;
}

export interface CreateForumTopicCommand {
  categoryId: number;
  lessonId?: number;
  title: string;
  subject: string;
  content: string;
}

@Injectable({
  providedIn: 'root'
})
export class ForumService extends BaseService {
  constructor(injector: Injector, private httpClient: HttpClient) {
    super(injector);
  }

  async getCategories(): Promise<IForumCategoryDto[]> {
    try {
      let url = `${this.urlApiNexas}ForumCategories`;
      const response = await firstValueFrom(
        this.httpClient.get<IForumCategoryDto[]>(url, this.GetAuthHeaderJson())
      );
      return this.extractData(response) as IForumCategoryDto[];
    } catch (error) {
      throw error;
    }
  }

  async getTopics(filters?: { categoryId?: number; lessonId?: number; status?: string; searchTitle?: string; page?: number; pageSize?: number }): Promise<IForumTopicSummaryDto[]> {
    try {
      let params = new HttpParams();
      if (filters) {
        if (filters.categoryId) params = params.set('CategoryId', filters.categoryId.toString());
        if (filters.lessonId) params = params.set('LessonId', filters.lessonId.toString());
        if (filters.status) params = params.set('Status', filters.status);
        if (filters.searchTitle) params = params.set('SearchTitle', filters.searchTitle);
        if (filters.page) params = params.set('Page', filters.page.toString());
        if (filters.pageSize) params = params.set('PageSize', filters.pageSize.toString());
      }
      
      let url = `${this.urlApiNexas}ForumTopics`;
      const requestOptions = this.GetAuthHeaderJson() as any;
      requestOptions.params = params;

      const response = await firstValueFrom(
        this.httpClient.get<IForumTopicSummaryDto[]>(url, requestOptions)
      );
      return this.extractData(response) as IForumTopicSummaryDto[];
    } catch (error) {
      throw error;
    }
  }

  async createTopic(command: CreateForumTopicCommand): Promise<{ id: number }> {
    try {
      let url = `${this.urlApiNexas}ForumTopics`;
      const response = await firstValueFrom(
        this.httpClient.post<{ id: number }>(url, command, this.GetAuthHeaderJson())
      );
      return this.extractData(response);
    } catch (error) {
      throw error;
    }
  }
}
