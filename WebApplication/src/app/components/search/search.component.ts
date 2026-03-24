import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImageGridComponent } from '../image-grid/image-grid.component';
import { ImageResource } from '../../infrastructure/resources/image.resource';
import { ImageSearchResult } from '../../models/images/image-search-result';
import { IndexingStatusResult } from '../../models/images/indexing-status-result';
import { UploadImageComponent } from '../upload-image/upload-image.component';

@Component({
    selector: 'app-search',
    standalone: true,
    imports: [CommonModule, FormsModule, ImageGridComponent, UploadImageComponent],
    templateUrl: './search.component.html',
    styleUrl: './search.component.css'
})
export class SearchComponent implements OnInit, OnDestroy {
  searchText = signal('');
  results = signal<ImageSearchResult[]>([]);
  status     = signal<IndexingStatusResult | null>(null);
  loading    = signal(false);
  searched   = signal(false);

  private resource = inject(ImageResource);
  
  private statusInterval: any;

  ngOnInit(): void {
     this.checkStatus();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  search(): void {
    if (!this.searchText().trim()) return;

    this.loading.set(true);
    this.searched.set(false);

    this.resource.search(this.searchText()).subscribe({
      next: results => {
        this.results.set(results);
        this.loading.set(false);
        this.searched.set(true);
      },
      error: () => {
        this.loading.set(false);
        this.searched.set(true);
      }
    });
  }

  checkStatus(): void {
    this.resource.getStatus().subscribe(status => {
      this.status.set(status);

      if (!status.ready) {
        this.statusInterval = setTimeout(() => this.checkStatus(), 3000);
      } else {
        this.stopPolling();
      }
    });
  }

  private stopPolling(): void {
    if (this.statusInterval) {
      clearTimeout(this.statusInterval);
      this.statusInterval = null;
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') this.search();
  }
}