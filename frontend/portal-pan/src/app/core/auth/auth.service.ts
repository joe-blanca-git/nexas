import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Injector } from '@angular/core';
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
  private user: UserLogedModel | null = null;

  constructor(protected override injector: Injector) {
    super(injector);
  }

  get loggedIn(): boolean {
    return this.authUtil.getCookieAuth() !== '';
  }

  logIn(email: string, password: string): Observable<UserLogedModel> {
    const url = `${this.urlApiServiceAuth}login`;
    const body = { email, password };

    const response = this.http
      .post(url, body, this.GetAuthHeaderJson())
      .pipe(map(this.extractData));

    return response;
  }

  async logOut() {
    this.authUtil.removeCookieAuth();
    this.stateUtil.clearState();
    sessionStorage.removeItem('nexas_user');
    this.user = null;
    await this.router.navigate(['/auth/login']);
  }

  async rehydrateUserState(): Promise<boolean> {
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
        this.user = hydrateUser;
        return true;
      }

      const hydratedUser: UserLogedModel = {
        email: payload.person.email || payload.user.email || null,
        name: payload.person.name || 'Usuário',
        username: payload.usuario || payload.unique_name || payload.name,
        id: payload.user.id || null,
        roles: payload.roles || [],
        idPerson: payload.person.id || null,
      };

      this.stateUtil.saveUser(hydratedUser);
      sessionStorage.setItem('nexas_user', JSON.stringify(hydratedUser));
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
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload?.exp * 1000;
      return Date.now() < exp;
    } catch (e) {
      return false;
    }
  }

  forgotPassword(email: string): Observable<any> {
    const url = `${this.urlApiServiceAuth}forgot-password`;
    const body = { email };

    return this.http
      .post(url, body, this.GetAuthHeaderJson())
      .pipe(map(this.extractData));
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<any> {
    const url = `${this.urlApiServiceAuth}reset-password`;
    const body = { email, token, newPassword };

    return this.http
      .post(url, body, this.GetAuthHeaderJson())
      .pipe(map(this.extractData));
  }

  checkEmail(email: string): Observable<any> {
    const url = `${this.urlApiServiceAuth}check-email/${encodeURIComponent(email)}`;
    return this.http.get(url, this.GetAuthHeaderJson());
  }

  registerSystemUser(payload: any): Observable<any> {
    const url = `${this.urlApiServiceAuth}register-system-user`;
    return this.http.post(url, payload, this.GetAuthHeaderJson());
  }
}