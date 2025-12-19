
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { StorageService } from '../storage/storage.service';
import { Router, NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-file-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './file-list.component.html',
  styleUrls: ['./file-list.component.css']
})
export class FileListComponent implements OnInit {

  private storage = inject(StorageService);
  private router = inject(Router);

  query = '';
  files: any[] = [];
  loading = false;
  isAdmin = false;

  page = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;

  ngOnInit() {
    const role = localStorage.getItem('role');
    this.isAdmin = role === 'Admin';

    // ✅ Load files ONLY after navigation is fully completed
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        if (localStorage.getItem('token')) {
          this.loadFiles();
        }
      }
    });
  }

  loadFiles() {
    this.loading = true;

    this.storage.getPagedFiles(this.page, this.pageSize).subscribe({
      next: (res) => {
        this.files = res.items;
        this.totalPages = res.totalPages;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) => {
        console.error("Error:", err);
        this.loading = false;
      }
    });
  }

  nextPage() {
    if (this.page < this.totalPages) {
      this.page++;
      this.loadFiles();
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.loadFiles();
    }
  }

  search() {
    this.loading = true;

    this.storage.list(this.query).subscribe({
      next: (res) => {
        this.files = res;
        this.loading = false;
      },
      error: (err) => {
        console.error("Error:", err);
        this.loading = false;
      }
    });
  }

  download(file: any) {
    this.storage.download(file.id).subscribe((blob: Blob) => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = file.name;
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  preview(id: string) {
    this.storage.preview(id).subscribe((blob: Blob) => {
      const url = URL.createObjectURL(blob);
      window.open(url, '_blank');
    });
  }

  softDelete(id: string) {
    this.storage.softDelete(id).subscribe(() => this.loadFiles());
  }

  hardDelete(id: string) {
    this.storage.hardDelete(id).subscribe(() => this.loadFiles());
  }
}



// import { Component, inject, OnInit } from '@angular/core';
// import { FormsModule } from '@angular/forms';
// import { CommonModule, DatePipe } from '@angular/common';
// import { StorageService } from '../storage/storage.service';
// import { Router, NavigationEnd } from '@angular/router';
// import { ChangeDetectorRef } from '@angular/core';


// @Component({
//   selector: 'app-file-list',
//   standalone: true,
//   imports: [CommonModule, FormsModule, DatePipe],
//   templateUrl: './file-list.component.html',
//   styleUrls: ['./file-list.component.css']
// })
// export class FileListComponent implements OnInit {
// private cdr = inject(ChangeDetectorRef);

//   private router = inject(Router);
//   private storage = inject(StorageService);

//   query = '';          // ✅ search text
//   files: any[] = [];
//   loading = false;
//   isAdmin = false;
//   page = 1;
//   pageSize = 10;totalPages = 0;totalCount = 0;

//   ngOnInit() {
//   const role = localStorage.getItem('role');
//   this.isAdmin = role === 'Admin';

//   // ✅ Load files ONLY after navigation is fully completed
//   this.router.events.subscribe(event => {
//     if (event instanceof NavigationEnd) {
//       if (localStorage.getItem('token')) {
//         this.loadFiles();
//       }
//     }
//   });
// }


//   loadFiles() {
//     this.loading = true;
//     this.storage.getPagedFiles(this.page, this.pageSize).subscribe({
//       next: (res) => {
//         this.files = res.items;
//         this.totalPages = res.totalPages;
//         this.totalCount = res.totalCount;
//         this.loading = false;
//         this.cdr.detectChanges();   // ✅ Force UI refresh

//       },
//       error: (err) => {
//         console.error("Error:", err);
//         this.loading = false;
//       }
//     });
//   }
//   nextPage() {
//     if (this.page < this.totalPages) {
//       this.page++;
//       this.loadFiles();
//     }
//   }

//   prevPage() {
//     if (this.page > 1) {
//       this.page--;
//       this.loadFiles();
//     }
//   }
//   // ✅ Search function
//   search() {
//     this.loading = true;

//     this.storage.list(this.query).subscribe({
//       next: (res) => {
//         this.files = res;
//         this.loading = false;
//       },
//       error: (err) => {
//         console.error("Error:", err);
//         this.loading = false;
//       }
//     });
//   }

//   download(file: any) {
//     this.storage.download(file.id).subscribe((blob: Blob) => {
//       const url = URL.createObjectURL(blob);
//       const a = document.createElement('a');
//       a.href = url;
//       a.download = file.name;
//       a.click();
//       URL.revokeObjectURL(url);
//     });
//   }

//   preview(id: string) {
//     this.storage.preview(id).subscribe((blob: Blob) => {
//       const url = URL.createObjectURL(blob);
//       window.open(url, '_blank');
//     });
//   }

//   softDelete(id: string) {
//     this.storage.softDelete(id).subscribe(() => this.search());
//   }

//   hardDelete(id: string) {
//     this.storage.hardDelete(id).subscribe(() => this.search());
//   }



// }
