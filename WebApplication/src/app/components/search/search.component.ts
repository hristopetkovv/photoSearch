import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImageGridComponent } from '../image-grid/image-grid.component';
import { ImageResource } from '../../infrastructure/resources/image.resource';
import { ImageResult } from '../../infrastructure/models/image-result';
import { StatusResult } from '../../infrastructure/models/status-result';

@Component({
    selector: 'app-search',
    standalone: true,
    imports: [CommonModule, FormsModule, ImageGridComponent],
    templateUrl: './search.component.html',
    styleUrl: './search.component.css'
})
export class SearchComponent implements OnInit, OnDestroy {
  searchText  = '';
  results:    ImageResult[] = [];
  status:     StatusResult | null = null;
  loading     = false;
  searched    = false;

  private statusInterval: any;

  constructor(private resource: ImageResource) {}

  ngOnInit(): void {
     this.checkStatus();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  private checkStatus(): void {
    this.resource.getStatus().subscribe(status => {
      this.status = status;

      if (!status.ready) {
        this.statusInterval = setTimeout(() => this.checkStatus(), 3000);
      } else {
        this.stopPolling();
      }
    });
  }

  search(): void {
    if (!this.searchText.trim()) return;

    this.loading  = true;
    this.searched = false;

    this.resource.search(this.searchText).subscribe({
      next: results => {
        this.results  = results;
        this.loading  = false;
        this.searched = true;
      },
      error: () => {
        this.loading  = false;
        this.searched = true;
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