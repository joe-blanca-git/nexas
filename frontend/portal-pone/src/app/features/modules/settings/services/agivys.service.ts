import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';

export interface AgivysRole {
  id: number;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class AgivysService extends BaseService {
  
  constructor(protected override injector: Injector, private http: HttpClient) {
    super(injector);
  }

  getRoles(): Observable<AgivysRole[]> {
    return this.http.get<AgivysRole[]>(`${this.urlApiService}RLS`, this.GetAuthHeaderJson());
  }

  assignRole(userId: number, roleName: string): Observable<any> {
    const payload = { userId, roleName };
    const options: any = this.GetAuthHeaderJson();
    options.responseType = 'text';
    return this.http.post(`${this.urlApiService}RLS/postAssignRole`, payload, options);
  }

  removeRole(userId: number, roleName: string): Observable<any> {
    const payload = { userId, roleName };
    const options: any = this.GetAuthHeaderJson();
    options.body = payload;
    options.responseType = 'text';
    return this.http.delete(`${this.urlApiService}RLS/removeRole`, options);
  }
}
