import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Injector, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { StateUtil } from '../utils/UserState.util';
import { BaseService } from '../services/base.service';
import { map, Observable } from 'rxjs';
import { UserLogedModel } from '../models/userLoged.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private stateUtil = inject(StateUtil);
  private platformId = inject(PLATFORM_ID);
  
  constructor(protected override injector: Injector) {
    super(injector);
  }

  get loggedIn(): boolean {
    return this.authUtil.getCookieAuth() !== '';
  }

  logIn(email: string, password: string): Observable<any> {
    const url = `${this.urlApiNexasAuth}login`; // Adjust to your actual endpoint if necessary
    const body = { email, password };

    return this.http
      .post(url, body, this.GetHeaderJson())
      .pipe(map(this.extractData));
  }

  googleLogin(idToken: string): Observable<any> {
    const url = `${this.urlApiNexasAuth}google-login`;
    const body = { idToken };

    return this.http
      .post(url, body, this.GetHeaderJson())
      .pipe(map(this.extractData));
  }

  async logOut() {
    this.authUtil.removeCookieAuth();
    this.stateUtil.clearState();
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.removeItem('nexas_user');
      await this.router.navigate(['/login']);
    }
  }

  async rehydrateUserState(): Promise<boolean> {
    if (!isPlatformBrowser(this.platformId)) {
      return true; 
    }

    const token = this.authUtil.getCookieAuth();

    if (!token) return false;

    try {
      if (!this.isTokenValid()) return false;

      const payload = this.authUtil.decodeToken(token);
      if (!payload) return false;

      const cachedUser = sessionStorage.getItem('nexas_user');

      if (cachedUser) {
        const hydrateUser = JSON.parse(cachedUser);
        this.stateUtil.saveUser(hydrateUser);
        return true;
      }

      const rolesFromToken = this.authUtil.getRolesFromToken(token);
      const rolesMapped = rolesFromToken.map((r: any) => ({ name: 'UserType', value: r }));

      const hydratedUser: UserLogedModel = {
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload?.email || null,
        name: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload?.name || 'Usuário',
        username: payload?.unique_name || payload?.name,
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload?.id || null,
        roles: rolesMapped
      };

      this.stateUtil.saveUser(hydratedUser);
      sessionStorage.setItem('nexas_user', JSON.stringify(hydratedUser));
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

  register(payload: any): Observable<any> {
    const url = `${this.urlApiNexasAuth}register`;
    return this.http.post(url, payload, this.GetHeaderJson());
  }

  forgotPassword(email: string): Observable<any> {
    const url = `${this.urlApiNexasAuth}forgotPassword`;
    return this.http.post(url, { email }, this.GetHeaderJson());
  }
}
