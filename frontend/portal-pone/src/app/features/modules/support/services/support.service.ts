import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, delay } from 'rxjs/operators';
import { BaseService } from '../../../../core/services/base.service';
import { SupportTicket, ForumTopic, MOCK_TICKETS, MOCK_TOPICS } from './support.mocks';

export type { ForumTopic, ForumMessage } from './support.mocks';

export interface IPaginatedList<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  totalPages: number;
}

export interface UserAccess {
  id: number;
  name: string;
  email: string;
  enrolledCoursesCount: number;
}

export interface PurchasedCourse {
  id: number;
  name: string;
  accessStatus: string;
  purchaseValue?: number;
  paymentMethod?: string;
  isFree?: boolean;
}

export interface IAdminTicket {
  id: number;
  subject: string;
  categoryName: string;
  studentName: string;
  studentEmail: string;
  status: string;
  createdAt: string;
  lastUpdatedAt: string;
  lastReplyAt?: string;
}

export interface IAdminTicketDetails extends IAdminTicket {
  messages: IAdminTicketMessage[];
}

export interface IAdminTicketMessage {
  id: number;
  content: string;
  origin: string; // 'Portal', 'Backoffice', 'Email'
  createdAt: string;
  senderName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SupportService extends BaseService {
  
  // Mock Data reference for stateful updates
  private mockTickets = MOCK_TICKETS;
  private mockTopics = MOCK_TOPICS;

  constructor(protected override injector: Injector, private http: HttpClient) {
    super(injector);
  }

  // --- TICKETS (CHAMADOS) ---

  getTickets(status?: string, categoryId?: number, searchTitle?: string): Observable<IPaginatedList<IAdminTicket>> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (categoryId) params = params.set('categoryId', categoryId.toString());
    if (searchTitle) params = params.set('searchText', searchTitle);

    return this.http.get<IPaginatedList<IAdminTicket>>(`${this.urlApiNexas}tickets`, {
      ...this.GetAuthHeaderJson(),
      params
    });
  }

  getTicketById(id: number): Observable<IAdminTicketDetails> {
    return this.http.get<IAdminTicketDetails>(`${this.urlApiNexas}tickets/${id}`, this.GetAuthHeaderJson());
  }

  replyTicket(id: number, content: string): Observable<{ messageId: number }> {
    const payload = { ticketId: id, content, attachments: [] };
    return this.http.post<{ messageId: number }>(`${this.urlApiNexas}tickets/${id}/messages`, payload, this.GetAuthHeaderJson());
  }

  updateTicketStatus(id: number, status: string): Observable<any> {
    const payload = { ticketId: id, status: parseInt(status) };
    return this.http.put(`${this.urlApiNexas}tickets/${id}/status`, payload, this.GetAuthHeaderJson());
  }

  // --- FORUM TOPICS ---

  getForumTopics(categoryId?: number, searchTitle?: string): Observable<any[]> {
    let params = new HttpParams();
    if (categoryId) params = params.set('categoryId', categoryId.toString());
    if (searchTitle) params = params.set('searchTitle', searchTitle);
    
    return this.http.get<any[]>(`${this.urlApiNexas}support-forum`, {
      ...this.GetAuthHeaderJson(),
      params
    });
  }

  getForumCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.urlApiNexas}ForumCategories`, this.GetAuthHeaderJson());
  }

  getForumTopicById(id: number): Observable<any> {
    return this.http.get<any>(`${this.urlApiNexas}support-forum/${id}`, this.GetAuthHeaderJson());
  }

  replyForumTopic(id: number, content: string): Observable<any> {
    const payload = { content };
    // The backend returns { id: resultId }
    return this.http.post<any>(`${this.urlApiNexas}support-forum/${id}/reply`, payload, this.GetAuthHeaderJson());
  }

  updateForumTopicStatus(id: number, status: string): Observable<any> {
    return this.http.patch(`${this.urlApiNexas}support-forum/${id}/status`, { status }, this.GetAuthHeaderJson());
  }

  // --- USER ACCESS ---

  getUsersAccess(): Observable<UserAccess[]> {
    return this.http.get<UserAccess[]>(`${this.urlApiNexas}support-access`, this.GetAuthHeaderJson());
  }

  getUserCourses(userId: number): Observable<PurchasedCourse[]> {
    return this.http.get<PurchasedCourse[]>(`${this.urlApiNexas}support-access/${userId}/courses`, this.GetAuthHeaderJson());
  }

  grantCourseAccess(userId: number, courseId: number): Observable<any> {
    return this.http.post(`${this.urlApiNexas}support-access/${userId}/grant/${courseId}`, {}, this.GetAuthHeaderJson());
  }

  revokeCourseAccess(userId: number, courseId: number): Observable<any> {
    return this.http.post(`${this.urlApiNexas}support-access/${userId}/revoke/${courseId}`, {}, this.GetAuthHeaderJson());
  }
}
