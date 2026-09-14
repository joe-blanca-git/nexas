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

  protected urlApiService: string = environment.apiAgivysUrl;
  protected urlApiServiceAuth: string = environment.apiAvivysAuthUrl;
  protected urlApiNexas: string = environment.apiNexasUrl;

  protected GetHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
    };
  }

  protected GetAuthHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + this.authUtil.getCookieAuth(),
      }),
    };
  }

  protected GetAuthHeaderTokenJson(token: string) {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + token,
      }),
    };
  }

  protected GetHeaderUnlercoded() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/x-www-form-urlencoded',
      }),
    };
  }

  protected GetAuthHeaderUploadJson() {
    return {
      headers: new HttpHeaders({
        Authorization: 'Bearer ' + this.authUtil.getCookieAuth(),
      }),
    };
  }

  protected extractData(response: any) {
    return response || {};
  }
}
