import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

import { TokenService }    from './token.service';
import {
  UserProfile, LoginRequest,
  AuthResponse, RegisterRequest,
} from '../models/auth.model';
import { API_ENDPOINTS, ROLE_ROUTES } from '../constants/api.constants';
import { BaseHttpService } from '../../services/base-http.service';
import { ApiResponse } from '../../models/api-response.model';
import { SignalrService } from '../../services/signalr.service';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private http         = inject(BaseHttpService);
  private tokenService = inject(TokenService);
  private router       = inject(Router);
  private signalrService = inject(SignalrService); //  INJECT ENGINE

  // ── Signals ───────────────────────────────────────────────
  private _currentUser = signal<UserProfile | null>(
    this.tokenService.getUser()
  );

  currentUser  = computed(() => this._currentUser());
  isLoggedIn   = computed(() => !!this._currentUser());
  currentRole  = computed(() => this._currentUser()?.role ?? null);

  // ── Custom constructor baseline initialization hook ─────────
  constructor() {
    //  Bootstrap persistent pipeline instantly if a session persists across app tabs
    if (this.isLoggedIn()) {
      this.signalrService.startConnection();
    }
  }

  // ── Login ─────────────────────────────────────────────────
  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<AuthResponse>('auth/login', request, {
      showSuccess: false,  // handled manually in login component
      showError:   true,
    }).pipe(
      tap(response => {
        if (response.success) this.handleAuthSuccess(response.data);
      })
    );
  }

  // ── Register ──────────────────────────────────────────────
  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<AuthResponse>('auth/register', request, {
      showSuccess: false,
      showError:   true,
    }).pipe(
      tap(response => {
        if (response.success) this.handleAuthSuccess(response.data);
      })
    );
  }

  // ── Refresh Token ─────────────────────────────────────────
  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = this.tokenService.getRefreshToken();
    return this.http.post<AuthResponse>('auth/refresh', { refreshToken }, {
      showSuccess: false,
      showError:   false,  // silent — interceptor handles 401 retry
    }).pipe(
      tap(response => {
        if (response.success) this.handleAuthSuccess(response.data);
      })
    );
  }
  
  forgotPassword(email: string): Observable<ApiResponse<boolean>> {
    return this.http.post<boolean>('auth/forgot-password', { email }, {
      showSuccess: false,
      showError:   false,   // handled manually in component
    });
  }
   
  validateResetToken(token: string): Observable<ApiResponse<boolean>> {
    return this.http.get<boolean>(`auth/validate-reset-token`, {
      params:    { token },
      showError: false,
    });
  }
   
  resetPassword(token: string, newPassword: string, confirmPassword: string)
    : Observable<ApiResponse<boolean>> {
    return this.http.post<boolean>('auth/reset-password',
      { token, newPassword, confirmPassword },
      { showSuccess: false, showError: false }
    );
  }
  // ── Logout ────────────────────────────────────────────────
  logout(): void {
    this.signalrService.stopConnection();

    const refreshToken = this.tokenService.getRefreshToken();
    if (refreshToken) {
      this.http.post<void>('auth/logout', { refreshToken }, {
        showSuccess: false,
        showError:   false,
      }).subscribe();
    }
    this.tokenService.clearAll();
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  navigateToDashboard(): void {
    const role = this.currentRole();
    if (role) this.router.navigate([ROLE_ROUTES[role] ?? '/login']);
  }

  private handleAuthSuccess(data: AuthResponse): void {
    this.tokenService.saveTokens(data.accessToken, data.refreshToken);
    this.tokenService.saveUser(data.user);
    localStorage.setItem('mustChangePassword', String(data.mustChangePassword));
    this._currentUser.set(data.user);

    this.signalrService.startConnection();
  }
}