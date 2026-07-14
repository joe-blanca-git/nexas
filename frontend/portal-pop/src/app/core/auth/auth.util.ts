import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class AuthUtil {

    public saveCookieAuth(response: any): void {
        const token = response.token;
        let expiresStr = '';
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            if (payload && payload.exp) {
                const expDate = new Date(payload.exp * 1000);
                expiresStr = `; expires=${expDate.toUTCString()}`;
            }
        } catch (e) {}
        document.cookie = `accessToken=${token}; path=/${expiresStr}; samesite=strict; secure`;
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