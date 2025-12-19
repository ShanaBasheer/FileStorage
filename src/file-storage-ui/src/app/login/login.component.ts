import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// 🔎 Define response type for clarity
interface LoginResponse {
  token: string;
  role?: string;
  message?: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  username = '';
  password = '';
  errorMessage = '';
  loading = false;

  login() {
    this.loading = true;
    this.errorMessage = '';

    this.auth.login(this.username, this.password).subscribe({
      next: (res: LoginResponse) => {
        this.loading = false;

        if (res.token) {

          // ✅ Save token
          this.auth.saveToken(res.token);

          // ✅ Save role (optional)
          if (res.role) {
            localStorage.setItem('role', res.role);
          }

          // ✅ ✅ IMPORTANT FIX
          // Give AppComponent time to update login state
          setTimeout(() => {
            this.router.navigate(['/file-list']);
          }, 50);

        } else {
          this.errorMessage = res.message || 'Login failed';
        }
      },

      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Invalid credentials';
      }
    });
  }
}




// import { Component, inject } from '@angular/core';
// import { Router } from '@angular/router';
// import { AuthService } from '../auth.service';
// import { CommonModule } from '@angular/common';
// import { FormsModule } from '@angular/forms';
 




// // 🔎 Define response type for clarity
// interface LoginResponse {
//   token: string;
//   role?: string;
//   message?: string;
// }

// @Component({
//   selector: 'app-login',
//     standalone: true,   // ✅ standalone component
//   imports: [FormsModule, CommonModule], // ✅ add modules here
//   templateUrl: './login.component.html',
//   styleUrls: ['./login.component.css']
// })
// export class LoginComponent {
//   private auth = inject(AuthService);
//   private router = inject(Router);

//   username = '';
//   password = '';
//   errorMessage = '';
//   loading = false;

//   login() {
//     this.loading = true;
//     this.errorMessage = '';

//     this.auth.login(this.username, this.password).subscribe({
//       next: (res: LoginResponse) => {
//         this.loading = false;

//         if (res.token) {
//           // ✅ Save token safely
//           this.auth.saveToken(res.token);

//           // ✅ Optional: save role if backend returns it
//           if (res.role) {
//             if (typeof window !== 'undefined' && window.localStorage) {
//               localStorage.setItem('role', res.role);
//             }
//           }

//           // ✅ Redirect after login
//           this.router.navigate(['/file-list']);
//         } else {
//           this.errorMessage = res.message || 'Login failed';
//         }
//       },
//       error: (err) => {
//         this.loading = false;
//         this.errorMessage = err.error?.message || 'Invalid credentials';
//       }
//     });
//   }
// }