import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageSearchResult } from '../../models/images/image-search-result';
import { Configuration } from '../../infrastructure/configuration/configuration';

@Component({
    selector: 'app-image-grid',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './image-grid.component.html',
    styleUrl: './image-grid.component.css'
})
export class ImageGridComponent {
  results = input<ImageSearchResult[]>([]);

  private configuration = inject(Configuration);

  getImageUrl(url: string): string {
    return `${this.configuration.mediaUrl}${url}`;
  }
}