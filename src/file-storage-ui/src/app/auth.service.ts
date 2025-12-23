
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {

 private apiUrl = 'http://localhost:5015/api/auth';


  constructor(private http: HttpClient) {}

  
  login(username: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, { username, password });
  }

 
  saveToken(token: string) {
    localStorage.setItem('token', token);   //  Use "token"
  }

 getToken(): string | null {
  if (typeof window === 'undefined') {
    return null;
  }
  return localStorage.getItem('token');
}



  logout() {
    localStorage.removeItem('token');
  }
}











//////////////////////////////////////update form by shana above 
// import { Injectable } from '@angular/core';
// import { HttpClient } from '@angular/common/http';
// import { Observable } from 'rxjs';

// @Injectable({ providedIn: 'root' })
// export class AuthService {
//   private apiUrl = 'http://localhost:5015/auth'; // ✅ adjust to your backend URL

//   constructor(private http: HttpClient) {}

//   // 🔑 Login API
//   login(username: string, password: string): Observable<any> {
//     return this.http.post<any>(`${this.apiUrl}/login`, { username, password });
//   }

//   // ✅ Save token safely
//   saveToken(token: string) {
//     if (typeof window !== 'undefined' && window.localStorage) {
//       localStorage.setItem('token', token);
//     }
//   }

//   // ✅ Get token safely
//   getToken(): string | null {
//     if (typeof window !== 'undefined' && window.localStorage) {
//       return localStorage.getItem('token');
//     }
//     return null;
//   }

//   // ✅ Logout safely
//   logout() {
//     if (typeof window !== 'undefined' && window.localStorage) {
//       localStorage.removeItem('token');
//     }
//   }
// }