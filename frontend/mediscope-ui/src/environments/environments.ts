// ════════════════════════════════════════════════════════════════
// FILE: src/app/core/tokens/http-context.tokens.ts
// ════════════════════════════════════════════════════════════════
import { HttpContextToken } from '@angular/common/http';

export const SHOW_SUCCESS = new HttpContextToken<boolean>(() => false);
export const SHOW_ERROR   = new HttpContextToken<boolean>(() => true);
// SHOW_ERROR defaults to true — errors are shown unless explicitly disabled
// SHOW_SUCCESS defaults to false — success toasts only when explicitly enabled


// ════════════════════════════════════════════════════════════════
// FILE: src/environments/environment.ts
// ════════════════════════════════════════════════════════════════
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5211/api',   // ← your backend port
};


// ════════════════════════════════════════════════════════════════
// FILE: src/app/core/models/api-response.model.ts
// ════════════════════════════════════════════════════════════════
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data:    T;
}