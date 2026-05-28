import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class AuthUtil {

    public saveCookieAuth(response: any): void {
        const token = response.token;
        document.cookie = `accessToken=${token}; path=/; samesite=strict; secure`;
    }

    public getCookieAuth(): string {
        if (typeof document === 'undefined') {
    return '';
  }
        const token = document.cookie
            .split('; ')
            .find(row => row.startsWith('accessToken='))
            ?.split('=')[1];
        return token || '';
    }

    public removeCookieAuth(): void {
        document.cookie = "accessToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    }

    public decodeToken(token: string): any {
        try {
            return JSON.parse(atob(token.split('.')[1]));
        } catch (e) {
            return null;
        }
    }
}