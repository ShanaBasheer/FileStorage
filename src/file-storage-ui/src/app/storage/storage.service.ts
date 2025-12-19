import { Injectable } from '@angular/core';
import { HttpClient,HttpHeaders  } from '@angular/common/http';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class StorageService {

 private apiUrl = 'http://localhost:5015/api/files';


  constructor(private http: HttpClient) {}

  // ✅ Safe header builder
 getHeaders() {
  return {
    headers: new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token') || ''}`
    })
  };
}

// ✅ List + Search
list(q: string = '') {
  return this.http
    .get<any[]>(`${this.apiUrl}/list?q=${q}`, this.getHeaders())
    .pipe(
      map(files =>
        files.map(f => ({
          id: f.id,
          name: f.name,
          size: f.size,
          contentType: f.contentType,
          createdAt: f.createdAt
        }))
      )
    );
}
 

// ✅ Download
download(id: string) {
  return this.http.get(`${this.apiUrl}/${id}/download`, {
    ...this.getHeaders(),
    responseType: 'blob'
  });
}

// ✅ Preview
preview(id: string) {
  return this.http.get(`${this.apiUrl}/${id}/preview`, {
    ...this.getHeaders(),
    responseType: 'blob'
  });
}

// ✅ Soft Delete
softDelete(id: string) {
  return this.http.delete(`${this.apiUrl}/${id}`, this.getHeaders());
}

// ✅ Hard Delete
hardDelete(id: string) {
  return this.http.delete(`${this.apiUrl}/${id}/hard`, this.getHeaders());
}


// upload(file: File, tags: string = '') {
//   const formData = new FormData();
//   formData.append('file', file);

//   return this.http.post(
//     `${this.apiUrl}/upload?tags=${tags}`,
//     formData,
//     {
//       headers: new HttpHeaders({
//         Authorization: `Bearer ${localStorage.getItem('token') || ''}`
//       })
//     }
//   );
// }

 upload(file: File, tags: string = '') {
  const formData = new FormData();
  formData.append('file', file);

  return this.http.post(
    `${this.apiUrl}/upload?tags=${tags}`,
    formData,
    {
      reportProgress: true,
      observe: 'events',
      headers: new HttpHeaders({
        Authorization: `Bearer ${localStorage.getItem('token') || ''}`
      })
    }
  );
}


getPagedFiles(page: number, pageSize: number) {
  return this.http.get<any>(
    `${this.apiUrl}/paged?page=${page}&pageSize=${pageSize}`,
    this.getHeaders()
  );
}

}






























// import { Injectable } from '@angular/core';
// import { HttpClient, HttpHeaders } from '@angular/common/http';
// import { map } from 'rxjs/operators';

// @Injectable({ providedIn: 'root' })
// export class StorageService {

//    private apiUrl = 'http://localhost:5015/api/files';
// // private apiUrl = 'http://localhost:5015/api/files/list';

//   constructor(private http: HttpClient) {}
// // ✅ Safe header builder
// private getHeaders() {
//   const token = localStorage.getItem('token');

//   return {
//     headers: {
//       Authorization: token ? `Bearer ${token}` : ''
//     }
//   };
// }

// // ✅ List files
// // list(q: string = '') {
// //   return this.http
// //     .get<any[]>(`${this.apiUrl}?q=${q}`, this.getHeaders())
// //     .pipe(
// //       map(files =>
// //         files.map(f => ({
// //           id: f.id,
// //           name: f.name,
// //           size: f.size,
// //           contentType: f.contentType,
// //           createdAt: f.createdAt
// //         }))
// //       )
// //     );
// // }

// list(q: string = '') {
//   return this.http
//     .get<any[]>(`${this.apiUrl}?q=${q}`, this.getHeaders())
//     .pipe(
//       map(files =>
//         files.map(f => ({
//           id: f.id,
//           name: f.name,
//           size: f.size,
//           contentType: f.contentType,
//           createdAt: f.createdAt
//         }))
//       )
//     );
// }

//   download(id: string) {
//     return this.http.get(`${this.apiUrl}/${id}/download`, {
//       ...this.getHeaders(),
//       responseType: 'blob'
//     });
//   }

//   preview(id: string) {
//     return this.http.get(`${this.apiUrl}/${id}/preview`, {
//       ...this.getHeaders(),
//       responseType: 'blob'
//     });
//   }

//   softDelete(id: string) {
//     return this.http.delete(`${this.apiUrl}/${id}`, this.getHeaders());
//   }

//   hardDelete(id: string) {
//     return this.http.delete(`${this.apiUrl}/${id}/hard`, this.getHeaders());
//   }
// }


// // import { Injectable } from '@angular/core';
// // import { HttpClient, HttpHeaders } from '@angular/common/http';
// // import { map } from 'rxjs/operators';

// // @Injectable({ providedIn: 'root' })
// // export class StorageService {
// //   private apiUrl = 'http://localhost:5015/api/files';


// //   constructor(private http: HttpClient) {}

// //   private getAuthHeaders(): HttpHeaders {
// //     const token = localStorage.getItem('token');
// //     return new HttpHeaders({ Authorization: `Bearer ${token}` });
// //   }
// // getHeaders() {
// //   const token = localStorage.getItem('token');

// //   return {
// //     headers: {
// //       Authorization: token ? `Bearer ${token}` : ''
// //     }
// //   };
// // }

// // // list(q: string = '') {
// // //   return this.http.get<any[]>(`${this.apiUrl}?q=${q}`, this.getHeaders());
// // // }

// //   // 🔎 List files
// //   list(q: string = '') {
// //     return this.http.get<any[]>(`${this.apiUrl}?q=${q}`, { headers: this.getAuthHeaders() })
// //       .pipe(
// //         map(files => files.map(f => ({
// //           id: f.id,
// //           name: f.name,          // ✅ match API
// //           size: f.size,          // ✅ match API
// //           contentType: f.contentType,
// //           createdAt: f.createdAt // ✅ match API
// //         })))
// //       );
// //   }

// //   download(id: string) {
// //     return this.http.get(`${this.apiUrl}/${id}/download`, {
// //       headers: this.getAuthHeaders(),
// //       responseType: 'blob'
// //     });
// //   }

// //   preview(id: string) {
// //     return this.http.get(`${this.apiUrl}/${id}/preview`, {
// //       headers: this.getAuthHeaders(),
// //       responseType: 'blob'
// //     });
// //   }

// //   softDelete(id: string) {
// //     return this.http.delete(`${this.apiUrl}/${id}`, { headers: this.getAuthHeaders() });
// //   }

// //   hardDelete(id: string) {
// //     return this.http.delete(`${this.apiUrl}/${id}/hard`, { headers: this.getAuthHeaders() });
// //   }
// // }