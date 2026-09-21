import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Injector, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { StateUtil, UserState } from '../utils/UserState.util';
import { BaseService } from '../services/base.service';
import { map, Observable } from 'rxjs';
import { UserLogedModel } from '../models/userLoged.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService extends BaseService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private stateUtil = inject(StateUtil);
  private platformId = inject(PLATFORM_ID);
  private user: UserLogedModel | null = null;

  constructor(protected override injector: Injector) {
    super(injector);
  }

  get loggedIn(): boolean {
    return this.authUtil.getCookieAuth() !== '';
  }

  logIn(email: string, password: string): Observable<UserLogedModel> {
    const url = `${this.urlApiNexas}apps/auth/login`;
    const body = { email, password };

    const response = this.http
      .post(url, body, this.GetHeaderJson())
      .pipe(map(this.extractData));

    return response;
  }

  async logOut() {
    this.authUtil.removeCookieAuth();
    this.stateUtil.clearState();
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.removeItem('pon_user');
      await this.router.navigate(['/auth/login']);
    }
    this.user = null;
  }

  async rehydrateUserState(): Promise<boolean> {
    if (!isPlatformBrowser(this.platformId)) {
      return true; // Skip hydrating user on server to prevent errors
    }

    const token = this.authUtil.getCookieAuth();

    if (!token) return false;

    try {
      if (!this.isTokenValid()) return false;

      const payload = this.authUtil.decodeToken(token);
      if (!payload) return false;

      const cachedUser = sessionStorage.getItem('pon_user');

      if (cachedUser) {
        const hydrateUser = JSON.parse(cachedUser);
        this.stateUtil.saveUser(hydrateUser);
        this.user = hydrateUser;
        return true;
      }

      const rolesFromToken = this.authUtil.getRolesFromToken(token);
      const rolesMapped = rolesFromToken.map(r => ({ name: 'UserType', value: r }));

      const hydratedUser: UserLogedModel = {
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload?.email || payload?.person?.email || payload?.user?.email || null,
        name: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload?.name || payload?.person?.name || 'Usuário',
        username: payload?.usuario || payload?.unique_name || payload?.name,
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload?.user?.id || payload?.id || null,
        roles: rolesMapped,
        idPerson: payload?.idPerson || payload?.person?.id || null,
      };

      this.stateUtil.saveUser(hydratedUser);
      sessionStorage.setItem('pon_user', JSON.stringify(hydratedUser));
      this.user = hydratedUser;
      return true;
    } catch (error) {
      console.error('Falha ao verificar dados do usuário:', error);
      this.logOut();
      return false;
    }
  }

  isTokenValid(): boolean {
    const token = this.authUtil.getCookieAuth();    

    if (!token) return false;

    try {
      const payload = this.authUtil.decodeToken(token);
      if (!payload || !payload.exp) return false;
      const exp = payload.exp * 1000;
      return Date.now() < exp;
    } catch (e) {
      return false;
    }
  }

  forgotPassword(email: string): Observable<any> {
    const url = `${this.urlApiNexas}apps/auth/forgot-password`;
    const body = { email }; // AppAuth no longer needs idSystem

    return this.http
      .post(url, body, this.GetHeaderJson())
      .pipe(map(this.extractData));
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<any> {
    const url = `${this.urlApiNexas}apps/auth/reset-password`;
    const body = { email, token, newPassword };

    return this.http
      .post(url, body, this.GetHeaderJson())
      .pipe(map(this.extractData));
  }

  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    // There is no explicit change-password in AppAuth documentation currently, keeping legacy/fallback path
    const url = `${this.urlApiNexas}apps/auth/change-password`; 
    const body = { currentPassword, newPassword };
    return this.http.post(url, body, this.GetAuthHeaderJson());
  }

  checkEmail(email: string): Observable<any> {
    const url = `${this.urlApiNexas}apps/auth/check-email/${encodeURIComponent(email)}`;
    return this.http.get(url, this.GetHeaderJson());
  }

  registerSystemUser(payload: any): Observable<any> {
    const url = `${this.urlApiNexas}apps/auth/register`;
    return this.http.post(url, payload, this.GetHeaderJson());
  }

  updatePerson(payload: any): Observable<any> {
    const url = `${this.urlApiNexas}person`;
    return this.http.put(url, payload, this.GetAuthHeaderJson());
  }

  getPerson(): Observable<any> {
    const url = `${this.urlApiNexas}person`;
    return this.http.get(url, this.GetAuthHeaderJson());
  }

  getMyAddresses(): Observable<any> {
    const url = `${this.urlApiNexas}address/my-address`;
    return this.http.get(url, this.GetAuthHeaderJson());
  }

  addAddress(payload: any): Observable<any> {
    const url = `${this.urlApiNexas}address/my-address`;
    return this.http.post(url, payload, this.GetAuthHeaderJson());
  }

  updateAddress(id: string, payload: any): Observable<any> {
    const url = `${this.urlApiNexas}address/my-address/${id}`;
    return this.http.put(url, payload, this.GetAuthHeaderJson());
  }

  deleteAddress(id: string): Observable<any> {
    const url = `${this.urlApiNexas}address/my-address/${id}`;
    return this.http.delete(url, this.GetAuthHeaderJson());
  }
}