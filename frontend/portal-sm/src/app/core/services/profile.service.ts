import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Injector } from '@angular/core';
import { map, Observable } from 'rxjs';
import { BaseService } from './base.service';

export interface ProfileModel {
  fullName: string;
  email: string;
  phoneNumber: string | null;
  avatar: string | null;
}

export interface UpdateProfilePayload {
  fullName: string;
  phoneNumber: string | null;
  avatar: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService extends BaseService {
  private http = inject(HttpClient);

  constructor(protected override injector: Injector) {
    super(injector);
  }

  getProfile(): Observable<ProfileModel> {
    const url = `${this.urlApiNexasAuth}profile`;
    return this.http.get<ProfileModel>(url, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  updateProfile(payload: UpdateProfilePayload): Observable<ProfileModel> {
    const url = `${this.urlApiNexasAuth}profile`;
    return this.http.put<ProfileModel>(url, payload, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }
}
