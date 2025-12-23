import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  isLoggedIn = false;
  isAdmin = false;
  currentUser: any = null;

 constructor(private router: Router) {
  this.router.events.subscribe(() => {
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');

    this.isLoggedIn = !!token;

    if (userData) {
      const user = JSON.parse(userData);
      this.currentUser = user;
      this.isAdmin = user?.role === 'Admin';
 }

    // NEW: Hide navbar on login and home pages
    const currentUrl = this.router.url;
    if (currentUrl === '/login' || currentUrl === '/home') {
      this.isLoggedIn = false;
    }

  });
}


  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.isLoggedIn = false;
    this.currentUser = null;
    this.router.navigate(['/login']);
  }
}
