import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  private auth = inject(AuthService);
  private router = inject(Router);

  user = '';
  password = '';
  errorMessage = '';
  loading = false;

  login() {
    this.loading = true;
    this.errorMessage = '';

    this.auth.login(this.user, this.password).subscribe({
      next: (res: any) => {
        this.loading = false;

        if (res.token) {
          localStorage.setItem('token', res.token);

          localStorage.setItem('role', res.role.toLowerCase());

          // FIXED: Save username from backend response
          localStorage.setItem('username', res.user);

          localStorage.setItem('user', JSON.stringify(res));

          this.router.navigate(['/home']);


        } else {
          this.errorMessage = 'Login failed';
        }
      },

      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Invalid credentials';
      }
    });
  }
}
