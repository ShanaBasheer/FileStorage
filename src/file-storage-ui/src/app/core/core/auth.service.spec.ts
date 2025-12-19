import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private token: string | null = null;
  private role: 'user' | 'admin' = 'user';

  login(username: string, role: 'user' | 'admin') {
    this.role = role;
    this.token = btoa(JSON.stringify({ sub: username, role }));
  }

  logout() { this.token = null; }
  getToken() { return this.token; }
  getRole() { return this.role; }
  isAdmin() { return this.role === 'admin'; }
  isLoggedIn() { return !!this.token; }
}