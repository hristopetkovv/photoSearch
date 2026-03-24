import { ApplicationConfig, inject, provideAppInitializer } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { Configuration } from './infrastructure/configuration/configuration';
import { provideToastr } from 'ngx-toastr';

export const appConfig: ApplicationConfig = {
  providers: [
    Configuration,
    provideAppInitializer(() => configFactory(inject(Configuration))),
    provideHttpClient(withInterceptorsFromDi()),
    provideToastr()
  ]
};

export function configFactory(config: Configuration) {
  return config.load();
}