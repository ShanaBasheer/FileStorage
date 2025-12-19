import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StorageService } from '../storage/storage.service';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent {

  private storage = inject(StorageService);

  selectedFile: File | null = null;
  tags: string = '';
  progress = 0;
  message = '';

  // ✅ File selection
  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  // ✅ Upload file
  uploadFile() {
    if (!this.selectedFile) {
      this.message = 'Please select a file first.';
      return;
    }

    this.progress = 10;
    this.message = 'Uploading...';

    this.storage.upload(this.selectedFile, this.tags).subscribe({
      next: () => {
        this.progress = 100;
        this.message = 'Upload successful!';
      },
      error: () => {
        this.message = 'Upload failed!';
      }
    });
  }
  isDragging = false;

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