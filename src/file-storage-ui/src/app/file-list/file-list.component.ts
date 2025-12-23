 import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { UploadComponent } from '../upload/upload.component';

interface FileItem {
  id: string;
  originalName: string;
  sizeBytes: number;
  contentType: string;
  createdAtUtc: string;
  tags?: string;
}

@Component({
  standalone: true,
  selector: 'app-file-list',
  imports: [CommonModule, FormsModule, UploadComponent   
],
  templateUrl: './file-list.component.html',
  styleUrls: ['./file-list.component.css'],
  
})
export class FileListComponent implements OnInit {

  role: string = '';

  // DATA
  files: FileItem[] = [];
  filteredFiles: FileItem[] = [];

  // FILTERS
  nameFilter = '';
contentTypeFilter = "";
fromDate = "";
toDate = "";

  // PAGINATION
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;

  private apiBase = 'http://localhost:5015/api/files';

 constructor(
  private http: HttpClient,
  private cdr: ChangeDetectorRef,
  private sanitizer: DomSanitizer
) {}

ngOnInit(): void {
    this.role = (localStorage.getItem('role') || '').toLowerCase();
    console.log("ROLE LOADED:", this.role);
   this.loadFiles();
  }
  // isLoading = false;

// BACKEND PAGINATION + FILTERS
loadFiles(): void {
    // this.isLoading = true;


  const url =
    `${this.apiBase}?page=${this.currentPage}` +
    `&pageSize=${this.pageSize}` +
    `&search=${this.nameFilter || ''}` +
    `&type=${this.contentTypeFilter || ''}` +
    `&fromDate=${this.fromDate || ''}` +
    `&toDate=${this.toDate || ''}`;

  this.http.get<any>(url).subscribe({
    next: res => {
      console.log("FILES RESPONSE:", res);

      this.filteredFiles = res.items || [];
      this.totalPages = Math.ceil(res.total / this.pageSize);

      this.cdr.detectChanges();   //  FIX 
    },
    error: err => console.error(err)
  });
}



resetFilters(): void {
  this.nameFilter = "";
  this.contentTypeFilter = "";
  this.fromDate = "";
  this.toDate = "";
  this.currentPage = 1;
  this.loadFiles();
}

  // //  BACKEND PAGINATION + FILTERS
  // loadFiles(): void {
  //   const params = new URLSearchParams({
  //     page: this.currentPage.toString(),
  //     pageSize: this.pageSize.toString(),
  //     search: this.nameFilter || ''
  //   });

  //   this.http.get<any>(`${this.apiBase}?${params.toString()}`).subscribe({
  //     next: res => {
  //       this.filteredFiles = res.items || [];
  //       this.totalPages = Math.ceil(res.total / this.pageSize);
  //     },
  //     error: err => console.error(err)
  //   });
  // }

  //  FILTER TRIGGER
applyFilters(): void {
  this.currentPage = 1;
  this.loadFiles();
} 

goToPage(page: number) {
  setTimeout(() => {
    this.currentPage = page;
    this.loadFiles();
  });
}

nextPage() {
  setTimeout(() => {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadFiles();
    }
  });
}

prevPage() {
  setTimeout(() => {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadFiles();
    }
  });
}
closePdf() {
  this.pdfUrl = null;
  this.cdr.detectChanges(); // ensures modal closes instantly
}

showToast(message: string) {
  const toast = document.getElementById('toast');
  if (!toast) return;

  toast.innerText = message;
  toast.classList.add('show');

  setTimeout(() => {
    toast.classList.remove('show');
  }, 2500);
}
pdfUrl: SafeResourceUrl | null = null;

// inline preview
previewFile(file: FileItem): void {
  const url = `${this.apiBase}/${file.id}/preview?download=true`;

  this.http.get(url, { responseType: 'blob' }).subscribe({
    next: blob => {
      const objectUrl = URL.createObjectURL(blob);

      setTimeout(() => {
        this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
        this.cdr.detectChanges();
      });
    },
    error: err => console.error(err)
  });
}




//hust commet for checking above code
//  pdfUrl: any = null;
// previewFile(file: FileItem): void {
//   if (file.contentType.includes('pdf') || file.contentType.includes('image')) {
//     // inline preview
//     const url = `${this.apiBase}/${file.id}/preview`;

//     this.http.get(url, { responseType: 'blob' }).subscribe({
//       next: blob => {
//         const objectUrl = URL.createObjectURL(blob);
//         this.pdfUrl = objectUrl; // inline modal
//       }
//     });
//   } else {
//     // fallback → open in new tab or download
//     this.downloadFile(file);
//   }
// }


// previewFile(file: FileItem): void {
//   const url = `${this.apiBase}/${file.id}/preview`;

//   this.http.get(url, { responseType: 'blob' }).subscribe({
//     next: blob => {
//       const objectUrl = URL.createObjectURL(blob);
//       this.pdfUrl = objectUrl;
//     }
//   });
// }

// closePdf() {
//   this.pdfUrl = null;
// }


  // // EXISTING FUNCTIONS (unchanged)
  // previewFile(file: FileItem): void {
  //   const url = `${this.apiBase}/${file.id}/preview`;

  //   this.http.get(url, { responseType: 'blob' }).subscribe({
  //     next: blob => {
  //       const objectUrl = URL.createObjectURL(blob);
  //       window.open(objectUrl, '_blank');
  //     }
  //   });
  // }

  downloadFile(file: FileItem): void {
    const url = `${this.apiBase}/${file.id}/download`;

    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: blob => {
        const a = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        a.href = objectUrl;
        a.download = file.originalName;
        a.click();
        URL.revokeObjectURL(objectUrl);
      },
      error: err => console.error('Error downloading file', err)
    });
  }

  softDeleteFile(file: FileItem): void {
    const url = `${this.apiBase}/${file.id}`;

    this.http.delete(url).subscribe({
      next: () => {
        this.loadFiles();
        this.showToast(`${file.originalName} soft deleted successfully`);

        
      },
      error: err => console.error('Error soft deleting file', err)
    });
  }

  hardDeleteFile(file: FileItem): void {
    const url = `${this.apiBase}/${file.id}/hard`;
     if (!confirm(`Permanently delete ${file.originalName}?`)) return;

    this.http.delete(url).subscribe({
      next: () => {
        this.loadFiles();
        this.showToast(`${file.originalName} permanently deleted`);
      
      },
      error: err => console.error('Error hard deleting file', err)
    });
  }
}
