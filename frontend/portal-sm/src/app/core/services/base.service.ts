import { Injectable, Injector } from '@angular/core';
import { AuthUtil } from '../auth/auth.util';
import { environment } from '../../../environments/environment';
import { HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export abstract class BaseService {
  constructor(protected injector: Injector) {}

  public get authUtil(): AuthUtil {
    return this.injector.get(AuthUtil);
  }

  protected urlApiNexas: string = environment.apiNexasUrl;
  protected urlApiNexasAuth: string = environment.apiNexasAuthUrl;

  protected GetHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };
  }

  protected GetAuthHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + this.authUtil.getCookieAuth()
      })
    };
  }

  protected extractData(response: any) {
    return response || {};
  }
}
