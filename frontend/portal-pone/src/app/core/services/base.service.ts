import { Injectable, Injector } from '@angular/core';
import { AuthUtil } from '../auth/auth.util';
import { environment } from '../environments/environment';
import { HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export abstract class BaseService {
  constructor(protected injector: Injector) {}

  public get authUtil(): AuthUtil {
    return this.injector.get(AuthUtil);
  }

  protected urlApiNexas: string = environment.apiNexasUrl;

  protected GetHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'X-Api-Key': environment.apiNexasKey,
      }),
    };
  }

  protected GetAuthHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + this.authUtil.getCookieAuth(),
        'X-Api-Key': environment.apiNexasKey,
      }),
    };
  }

  protected GetAuthHeaderTokenJson(token: string) {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + token,
        'X-Api-Key': environment.apiNexasKey,
      }),
    };
  }

  protected GetHeaderUnlercoded() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Api-Key': environment.apiNexasKey,
      }),
    };
  }

  protected GetAuthHeaderUploadJson() {
    return {
      headers: new HttpHeaders({
        Authorization: 'Bearer ' + this.authUtil.getCookieAuth(),
        'X-Api-Key': environment.apiNexasKey,
      }),
    };
  }

  protected extractData(response: any) {
    return response || {};
  }
}
