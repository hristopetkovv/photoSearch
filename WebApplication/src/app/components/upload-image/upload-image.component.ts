import { Component, inject, output, signal } from '@angular/core';
import { ImageResource } from '../../infrastructure/resources/image.resource';
import { ImageUploadResult } from '../../models/images/image-upload-result';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-upload-image',
    standalone: true,
    templateUrl: './upload-image.component.html',
    styleUrl: './upload-image.component.css'
})
export class UploadImageComponent {
    uploading = signal(false);

    private resource = inject(ImageResource);
    private toastr = inject(ToastrService);

    uploaded = output<void>();

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file) return;

        this.uploading.set(true);

        this.resource.upload(file).subscribe({
            next: (result: ImageUploadResult) => {
                this.uploading.set(false);

                if (result.success) {
                    this.toastr.success(result.message, 'Успех');
                    this.uploaded.emit();
                    input.value = '';
                } else {
                    this.toastr.error(result.message, 'Грешка');
                }
                
            },
            error: () => {
                this.uploading.set(false);
                this.toastr.error('Грешка при качване.', 'Грешка');
            }
        });
    }
}