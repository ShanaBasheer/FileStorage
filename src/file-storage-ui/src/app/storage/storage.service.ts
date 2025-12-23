import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class StorageService {

  private apiUrl = 'http://localhost:5015/api/files';


  constructor(private http: HttpClient) { }

  getHeaders() {
    return {
      headers: new HttpHeaders({
        Authorization: `Bearer ${localStorage.getItem('token') || ''}`
      })
    };
  }

  list(
    page: number,
    pageSize: number,
    search: string = '',
    type: string = '',
    fromDate: string = '',
    toDate: string = ''
  ) {
    return this.http.get<any>(
      `${this.apiUrl}?page=${page}` +
      `&pageSize=${pageSize}` +
      `&search=${search}` +
      `&type=${type}` +
      `&fromDate=${fromDate}` +
      `&toDate=${toDate}`,
      this.getHeaders()
    );
  }

  download(id: string) {
    return this.http.get(`${this.apiUrl}/${id}/download`, {
      ...this.getHeaders(),
      responseType: 'blob'
    });
  }

  preview(id: string) {
    return this.http.get(`${this.apiUrl}/${id}/preview`, {
      ...this.getHeaders(),
      responseType: 'blob'
    });
  }

  softDelete(id: string) {
    return this.http.delete(`${this.apiUrl}/${id}`, this.getHeaders());
  }

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
    formData.append('tags', tags);

    return this.http.post(
      `${this.apiUrl}/upload`,
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



























