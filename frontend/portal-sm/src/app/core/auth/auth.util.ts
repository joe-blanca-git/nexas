import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthUtil {
  private readonly COOKIE_NAME = 'nexas_accessToken';

  public saveCookieAuth(response: any): void {
    const token = response.accessToken || response.token;
    let expiresStr = '';
    try {
      let base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      while (base64.length % 4) {
        base64 += '=';
      }
      const payload = JSON.parse(atob(base64));
      if (payload && payload.exp) {
        const expDate = new Date(payload.exp * 1000);
        expiresStr = `; expires=${expDate.toUTCString()}`;
      }
    } catch (e) {}
    document.cookie = `${this.COOKIE_NAME}=${token}; path=/${expiresStr}; samesite=strict; secure`;
  }

  public getCookieAuth(): string {
    if (typeof document === 'undefined') {
      return '';
    }
    const token = document.cookie
      .split('; ')
      .find(row => row.startsWith(`${this.COOKIE_NAME}=`))
      ?.split('=')[1];
    return token || '';
  }

  public removeCookieAuth(): void {
    document.cookie = `${this.COOKIE_NAME}=; Max-Age=-99999999; path=/; samesite=strict; secure`;
  }

  public decodeToken(token: string): any {
    try {
      if (!token) return null;
      let base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      while (base64.length % 4) {
        base64 += '=';
      }
      return JSON.parse(atob(base64));
    } catch (e) {
      return null;
    }
  }

  public getRolesFromToken(token: string): string[] {
    const payload = this.decodeToken(token);
    if (!payload) return [];
    
    // In many .NET identity setups, role is defined under this claim schema
    const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || payload.roles;
    
    if (!roleClaim) return [];
    
    if (Array.isArray(roleClaim)) {
      return roleClaim;
    }
    
    return [roleClaim];
  }

  public hasRole(roleName: string): boolean {
    const token = this.getCookieAuth();
    if (!token) return false;
    const roles = this.getRolesFromToken(token);
    return roles.includes(roleName);
  }
}
