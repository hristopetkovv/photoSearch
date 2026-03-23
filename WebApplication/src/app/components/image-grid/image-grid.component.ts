import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageSearchResult } from '../../infrastructure/models/image-search-result';
import { Configuration } from '../../infrastructure/configuration/configuration';

@Component({
    selector: 'app-image-grid',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './image-grid.component.html',
    styleUrl: './image-grid.component.css'
})
export class ImageGridComponent {
  @Input() results: ImageSearchResult[] = [];

  constructor(private configuration: Configuration)
  { }

  getImageUrl(url: string): string {
    return `${this.configuration.mediaUrl}${url}`;
  }
}