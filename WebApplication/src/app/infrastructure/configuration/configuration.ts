import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

@Injectable()
export class Configuration {

    clientUrl!: string;
    restUrl!: string;
    mediaUrl!: string;

    constructor(
        private httpClient: HttpClient
    ) { }

    load(): Promise<{}> {
    return new Promise((resolve) => {
        this.httpClient.get('configuration.json')
            .subscribe({
                next: (config) => {
                    this.importSettings(config);
                    resolve({});
                },
                error: (err) => {
                    console.error('Грешка при зареждане на конфигурацията', err);
                    resolve({});
                }
            });
    });
}

    private importSettings(config: any) {
        if (config.restUrlFromAppOrigin) {
            config.urls.restUrl = `${window.location.origin}/api`;
            config.urls.clientUrl = `${window.location.origin}`;
            config.urls.mediaUrl  = `${window.location.origin}`;
        }

        this.restUrl   = config.urls.restUrl;
        this.clientUrl = config.urls.clientUrl;
        this.mediaUrl  = config.urls.mediaUrl;
    }
}