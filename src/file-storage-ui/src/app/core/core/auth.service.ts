import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private role: 'user' | 'admin' = 'user';

  login(username: string, role: 'user' | 'admin') {
    this.role = role;
    const token = btoa(JSON.stringify({ sub: username, role }));

    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
      localStorage.setItem('role', role);
    }
  }

  logout() {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('role');
    }
  }

  getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('token');
    }
    return null;
  }

  getRole(): 'user' | 'admin' {
    if (typeof window !== 'undefined') {
      return (localStorage.getItem('role') as 'user' | 'admin') || 'user';
    }
    return 'user';
  }

  isAdmin() { return this.getRole() === 'admin'; }
  isLoggedIn() { return !!this.getToken(); }
}