
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpEvent, HttpEventType, HttpResponse } from '@angular/common/http';
import { StorageService } from '../storage/storage.service';
import { ChangeDetectorRef, Component, EventEmitter, Output, inject } from '@angular/core';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent {

  messageType: 'success' | 'error' | '' = '';
  message = '';
  progress = 0;

  selectedFile: File | null = null;
  tags: string = '';
  isDragging = false;

  fileInput!: HTMLInputElement;

  private storage = inject(StorageService);

  @Output() uploaded = new EventEmitter<void>();

  constructor(private cdr: ChangeDetectorRef) { }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.fileInput = event.target;

    this.clearMessages();

    if (this.selectedFile) {
      this.message = `Selected: ${this.selectedFile.name}`;
    }
  }

  uploadFile() {
    if (!this.selectedFile) {
      this.showError('Please select a file first.');
      return;
    }

    this.progress = 0;
    this.message = 'Uploading...';
    this.messageType = '';

    this.storage.upload(this.selectedFile, this.tags).subscribe({
      next: (event: HttpEvent<any>) => {

        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.progress = Math.round((event.loaded / event.total) * 100);
        }

        if (event instanceof HttpResponse) {
          this.progress = 100;
          this.showSuccess('Upload successful!');
          this.uploaded.emit();

          setTimeout(() => this.resetUploadUI(), 1500);
        }
      },

      error: (err) => {
        console.error(err);
        this.progress = 0;


        if (err.status === 0) {
          this.showError('Connection lost. Upload failed.');
          return;
        }

        if (err.status === 413) {
          this.showError('File too large. Maximum size exceeded.');
          return;
        }

        if (err.status === 415) {
          this.showError('Unsupported file type.');
          return;
        }

        this.showError('Upload failed! Please try again.');
      }
    });
  }


  resetUploadUI() {
    this.selectedFile = null;
    this.tags = '';
    this.progress = 0;

    if (this.fileInput) {
      this.fileInput.value = '';
    }

    this.cdr.detectChanges();
  }

  clearMessages() {
    this.message = '';
    this.messageType = '';
  }

  showSuccess(msg: string) {
    this.message = msg;
    this.messageType = 'success';
  }

  showError(msg: string) {
    this.message = msg;
    this.messageType = 'error';
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;

    if (event.dataTransfer?.files.length) {
      this.selectedFile = event.dataTransfer.files[0];
      this.message = `Selected: ${this.selectedFile.name}`;
    }
  }
}
