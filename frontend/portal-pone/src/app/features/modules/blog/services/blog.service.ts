import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';

export interface BlogPostDto {
  id: number;
  title: string;
  subject: string;
  content: string;
  tags?: string;
  headerImageUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateBlogPostCommand {
  title: string;
  subject: string;
  content: string;
  tags?: string;
  headerImageUrl?: string;
}

export interface UpdateBlogPostCommand extends CreateBlogPostCommand {
  id: number;
}

@Injectable({
  providedIn: 'root'
})
export class BlogService extends BaseService {
  constructor(protected override injector: Injector, private http: HttpClient) {
    super(injector);
  }

  getAll(): Observable<BlogPostDto[]> {
    return this.http.get<BlogPostDto[]>(`${this.urlApiNexas}BlogPosts`, this.GetAuthHeaderJson());
  }

  getById(id: number): Observable<BlogPostDto> {
    return this.http.get<BlogPostDto>(`${this.urlApiNexas}BlogPosts/${id}`, this.GetAuthHeaderJson());
  }

  create(command: CreateBlogPostCommand): Observable<number> {
    return this.http.post<number>(`${this.urlApiNexas}BlogPosts`, command, this.GetAuthHeaderJson());
  }

  update(id: number, command: UpdateBlogPostCommand): Observable<void> {
    return this.http.put<void>(`${this.urlApiNexas}BlogPosts/${id}`, command, this.GetAuthHeaderJson());
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlApiNexas}BlogPosts/${id}`, this.GetAuthHeaderJson());
  }

  uploadImage(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.urlApiNexas}uploads/image?folder=blog/covers`, formData, this.GetAuthHeaderUploadJson());
  }
}
