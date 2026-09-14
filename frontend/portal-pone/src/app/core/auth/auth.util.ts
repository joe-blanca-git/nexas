import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class AuthUtil {

    public saveCookieAuth(response: any): void {
        const token = response.token;
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
        document.cookie = `pon_accessToken=${token}; path=/${expiresStr}; samesite=strict; secure`;
    }

    public getCookieAuth(): string {
        if (typeof document === 'undefined') {
    return '';
  }
        const token = document.cookie
            .split('; ')
            .find(row => row.startsWith('pon_accessToken='))
            ?.split('=')[1];
        return token || '';
    }

    public removeCookieAuth(): void {
        document.cookie = "pon_accessToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    }

    public decodeToken(token: string): any {
        try {
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
        if (!token) return [];
        const decoded = this.decodeToken(token);
        if (!decoded) return [];

        const rolesClaim = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                        || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role']
                        || decoded['roles']
                        || decoded['role']
                        || [];

        if (Array.isArray(rolesClaim)) {
            return rolesClaim.map((r: any) => typeof r === 'string' ? r : (r.value || r.name || String(r)));
        } else if (typeof rolesClaim === 'string') {
            return [rolesClaim];
        }

        return [];
    }

    public getRoles(): string[] {
        const roles: string[] = [];
        const token = this.getCookieAuth();
        if (token) {
            roles.push(...this.getRolesFromToken(token));
        }

        if (roles.length === 0 && typeof sessionStorage !== 'undefined') {
            try {
                const cachedUserStr = sessionStorage.getItem('pon_user');
                if (cachedUserStr) {
                    const cachedUser = JSON.parse(cachedUserStr);
                    const userRoles = cachedUser?.user?.roles || cachedUser?.roles || [];
                    if (Array.isArray(userRoles)) {
                        userRoles.forEach((r: any) => {
                            if (typeof r === 'string') {
                                roles.push(r);
                            } else if (r && (r.value || r.name)) {
                                roles.push(r.value || r.name);
                            }
                        });
                    }
                }
            } catch (e) {}
        }

        return Array.from(new Set(roles));
    }

    public hasRole(roleName: string): boolean {
        if (!roleName) return false;
        const roles = this.getRoles();
        return roles.some(r => r.toLowerCase() === roleName.toLowerCase());
    }
}