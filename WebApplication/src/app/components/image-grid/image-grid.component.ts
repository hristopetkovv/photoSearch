import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageResult } from '../../infrastructure/models/image-result';
import { Configuration } from '../../infrastructure/configuration/configuration';

@Component({
    selector: 'app-image-grid',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './image-grid.component.html',
    styleUrl: './image-grid.component.css'
})
export class ImageGridComponent {
  @Input() results: ImageResult[] = [];

  constructor(private configuration: Configuration)
  { }

  getImageUrl(url: string): string {
    return `${this.configuration.mediaUrl}${url}`;
  }
}