import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Injector } from '@angular/core';
import { map, Observable } from 'rxjs';
import { BaseService } from './base.service';

export interface PrivacyPreferences {
  receiveMarketingEmails: boolean;
  receiveProductNotifications: boolean;
}

export interface UserDataExport {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  emailConfirmed: boolean;
  roles: string[];
  receiveMarketingEmails: boolean;
  receiveProductNotifications: boolean;
  avatarBase64: string | null;
  loginHistory: { method: string; loginDate: string }[];
}

@Injectable({
  providedIn: 'root'
})
export class PrivacyService extends BaseService {
  private http = inject(HttpClient);

  constructor(protected override injector: Injector) {
    super(injector);
  }

  getPreferences(): Observable<PrivacyPreferences> {
    const url = `${this.urlApiNexasAuth}privacy`;
    return this.http.get<PrivacyPreferences>(url, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  updatePreferences(payload: PrivacyPreferences): Observable<PrivacyPreferences> {
    const url = `${this.urlApiNexasAuth}privacy`;
    return this.http.put<PrivacyPreferences>(url, payload, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  exportData(): Observable<UserDataExport> {
    const url = `${this.urlApiNexasAuth}export-data`;
    return this.http.get<UserDataExport>(url, this.GetAuthHeaderJson()).pipe(map(this.extractData));
  }

  deleteAccount(confirmEmail: string): Observable<void> {
    const url = `${this.urlApiNexasAuth}account?confirmEmail=${encodeURIComponent(confirmEmail)}`;
    return this.http.delete<void>(url, this.GetAuthHeaderJson());
  }
}
