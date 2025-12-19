
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';

import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { authInterceptor } from './app/core/interceptors/auth.interceptor';
import { withRouterConfig } from '@angular/router';

 

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(
      routes,
      withRouterConfig({
        onSameUrlNavigation: 'reload'
      })
    ),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    )
  ]
});

// import { bootstrapApplication } from '@angular/platform-browser';
// import { provideRouter } from '@angular/router';
// import { provideHttpClient, withInterceptors } from '@angular/common/http';

// import { AppComponent } from './app/app.component';
// import { routes } from './app/app.routes';
// import { authInterceptor } from './app/core/interceptors/auth.interceptor';

// bootstrapApplication(AppComponent, {
//   providers: [
//     provideRouter(routes),
//     provideHttpClient(withInterceptors([authInterceptor]))
//   ]
// });
