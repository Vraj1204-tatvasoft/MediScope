import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { map } from 'rxjs/operators';
import { NotificationDto } from '../models/notification.model';
import { SignalrService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class PatientNotificationService {
  private http = inject(HttpClient);
  private signalrService = inject(SignalrService);

  private readonly API_URL = 'http://localhost:5211/api/notifications';

  // Global reactive counters to power layout header badges instantly
  unreadCount = signal<number>(0);

  constructor() {
    // Listen for incoming live WebSockets events to increment count badges automatically
    this.signalrService.notification$.subscribe({
      next: () => {
        this.unreadCount.update(count => count + 1);
      }
    });
  }

  getNotifications(): Observable<NotificationDto[]> {
    return this.http.get<{ data: NotificationDto[] }>(this.API_URL).pipe(
      map(response => response.data)
    );
  }

  syncUnreadCount(): void {
    this.http.get<{ data: number }>(`${this.API_URL}/unread-count`).subscribe({
      next: (res) => this.unreadCount.set(res.data || 0)
    });
  }
  markAsRead(notificationId: string): Observable<any> {
    return this.http.post(`${this.API_URL}/${notificationId}/read`, {}).pipe(
      tap(() => {
        this.unreadCount.update(count => Math.max(0, count - 1));
      })
    );
  }
  markAllAsRead(): Observable<any> {
    return this.http.post(`${this.API_URL}/mark-all-read`, {}).pipe(
      tap(() => this.unreadCount.set(0))
    );
  }
  clearAllNotifications(): Observable<any> {
    return this.http.delete(`${this.API_URL}/clear-all`).pipe(
      tap(() => {
        this.unreadCount.set(0);
      })
    );
  }
}