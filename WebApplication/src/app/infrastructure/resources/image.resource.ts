import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ImageSearchResult } from "../models/images/image-search-result";
import { Observable } from "rxjs";
import { IndexingStatusResult } from "../models/images/indexing-status-result";

@Injectable({ providedIn: 'root' })
export class ImageResource {

    url = 'api/image';

    constructor(
        private http: HttpClient
    ) { }

    search(text: string, limit: number = 20): Observable<ImageSearchResult[]> {
        return this.http.get<ImageSearchResult[]>(`${this.url}/search?text=${encodeURIComponent(text)}&limit=${limit}&t=${Date.now()}`);
      }
    
    getStatus(): Observable<IndexingStatusResult> {
      return this.http.get<IndexingStatusResult>(`${this.url}/status`);
    }
}