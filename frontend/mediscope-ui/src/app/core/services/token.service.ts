import { Injectable } from '@angular/core';
import { UserProfile } from '../models/auth.model';
 
const ACCESS_TOKEN_KEY  = 'ms_access_token';
const REFRESH_TOKEN_KEY = 'ms_refresh_token';
const USER_KEY          = 'ms_user';
 
@Injectable({ providedIn: 'root' })
export class TokenService {
 
  saveTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY,  accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  }
 
  saveUser(user: UserProfile): void {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }
 
  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }
 
  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }
 
  getUser(): UserProfile | null {
    const user = localStorage.getItem(USER_KEY);
    return user ? JSON.parse(user) : null;
  }
 
  getRole(): string | null {
    return this.getUser()?.role ?? null;
  }
 
  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }
 
  clearAll(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}