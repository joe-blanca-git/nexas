import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, Injector } from '@angular/core';
import { map, Observable } from 'rxjs';
import { BaseService } from './base.service';

export interface ApplicationModel {
  id: string;
  name: string;
  description: string | null;
  urlLogo: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
  urlDomain: string | null;
  status: string;
  apiKey: string;
  googleClientId: string | null;
  createdAt: string;
  createdUser: string | null;
  updatedAt: string | null;
  updatedUser: string | null;
}

export interface CreateApplicationPayload {
  name: string;
  description: string | null;
  urlLogo: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
  urlDomain: string | null;
  status: string;
}

export interface AppRoleModel {
  id: string;
  name: string;
  createdAt: string;
}

export interface ApplicationEndUserModel {
  id: string;
  email: string;
  fullName: string | null;
  createdAt: string;
  roles: AppRoleModel[];
}

export interface CreateEndUserPayload {
  email: string;
  password: string;
  fullName: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class ApplicationsService extends BaseService {
  private http = inject(HttpClient);

  constructor(protected override injector: Injector) {
    super(injector);
  }

  getAll(): Observable<ApplicationModel[]> {
    const url = `${this.urlApiNexas}v1/Applications`;
    return this.http.get<ApplicationModel[]>(url, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  getById(id: string): Observable<ApplicationModel> {
    const url = `${this.urlApiNexas}v1/Applications/${id}`;
    return this.http.get<ApplicationModel>(url, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  create(payload: CreateApplicationPayload): Observable<ApplicationModel> {
    const url = `${this.urlApiNexas}v1/Applications`;
    return this.http.post<ApplicationModel>(url, payload, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  regenerateApiKey(id: string): Observable<ApplicationModel> {
    const url = `${this.urlApiNexas}v1/Applications/${id}/api-key/regenerate`;
    return this.http.post<ApplicationModel>(url, {}, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  updateGoogleClientId(id: string, googleClientId: string | null): Observable<ApplicationModel> {
    const url = `${this.urlApiNexas}v1/Applications/${id}`;
    return this.http
      .patch<ApplicationModel>(url, { googleClientId: googleClientId ?? '' }, this.GetAuthHeaderJson())
      .pipe(map(this.extractData));
  }

  delete(id: string): Observable<void> {
    const url = `${this.urlApiNexas}v1/Applications/${id}`;
    return this.http.delete<void>(url, this.GetAuthHeaderJson());
  }

  getUsers(id: string): Observable<ApplicationEndUserModel[]> {
    const url = `${this.urlApiNexas}v1/Applications/${id}/users`;
    return this.http.get<ApplicationEndUserModel[]>(url, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  createEndUser(appId: string, apiKey: string, payload: CreateEndUserPayload): Observable<ApplicationEndUserModel> {
    const url = `${this.urlApiNexas}v1/apps/auth/register`;
    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'X-Api-Key': apiKey
      })
    };
    return this.http.post<ApplicationEndUserModel>(url, payload, options).pipe(map(this.extractData));
  }

  getRoles(appId: string): Observable<AppRoleModel[]> {
    const url = `${this.urlApiNexas}v1/Applications/${appId}/roles`;
    return this.http.get<AppRoleModel[]>(url, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  createRole(appId: string, name: string): Observable<AppRoleModel> {
    const url = `${this.urlApiNexas}v1/Applications/${appId}/roles`;
    return this.http.post<AppRoleModel>(url, { name }, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  deleteRole(appId: string, roleId: string): Observable<void> {
    const url = `${this.urlApiNexas}v1/Applications/${appId}/roles/${roleId}`;
    return this.http.delete<void>(url, this.GetAuthHeaderJson());
  }

  assignRoleToUser(appId: string, userId: string, roleId: string): Observable<void> {
    const url = `${this.urlApiNexas}v1/Applications/${appId}/users/${userId}/roles/${roleId}`;
    return this.http.post<void>(url, {}, this.GetAuthHeaderJson());
  }

  removeRoleFromUser(appId: string, userId: string, roleId: string): Observable<void> {
    const url = `${this.urlApiNexas}v1/Applications/${appId}/users/${userId}/roles/${roleId}`;
    return this.http.delete<void>(url, this.GetAuthHeaderJson());
  }
}
