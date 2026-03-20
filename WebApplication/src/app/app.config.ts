import { ApplicationConfig, inject, provideAppInitializer } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { Configuration } from './infrastructure/configuration/configuration';

export const appConfig: ApplicationConfig = {
  providers: [
    Configuration,
    provideAppInitializer(() => configFactory(inject(Configuration))),
    provideHttpClient(withInterceptorsFromDi())
  ]
};

export function configFactory(config: Configuration) {
  return config.load();
}