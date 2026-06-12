import { Component, OnInit, signal, computed, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';
import { SignalrService } from '../../services/signalr.service';
import { NotificationDto } from '../../models/notification.model';
import { PatientNotificationService } from '../../services/patient-notification.service';

export type FilterType = 'ALL' | 'UNREAD' | 'ALERTS';

@Component({
  selector: 'app-patient-notification',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './patient-notification.component.html',
  styleUrls: ['./patient-notification.component.css']
})
export class PatientNotificationComponent implements OnInit, OnDestroy {
  private notificationService = inject(PatientNotificationService);
  private signalrService = inject(SignalrService);
  private liveSubscription!: Subscription;

  isLoading = signal<boolean>(true);
  rawNotifications = signal<NotificationDto[]>([]);
  activeFilter = signal<FilterType>('ALL');

  // Compute counters reactively based on live data updates
  totalCount = computed(() => this.rawNotifications().length);
  unreadCount = computed(() => this.notificationService.unreadCount());
  
  alertCount = computed(() => 
    this.rawNotifications().filter(n => n.type.toLowerCase() === 'alert').length
  );

  // ── FILTER COMPUTE PIPELINE ──────────────────────────────────────
  filteredNotifications = computed(() => {
    const list = this.rawNotifications();
    const filter = this.activeFilter();

    switch(filter) {
      case 'UNREAD':
        return list.filter(n => !n.isRead);
      case 'ALERTS':
        return list.filter(n => n.type.toLowerCase() === 'alert');
      default:
        return list;
    }
  });

  ngOnInit(): void {
    this.loadHistoricalNotifications();
    this.notificationService.syncUnreadCount();

    // Attach runtime handler hook to accept live items pushed down via SignalR
    this.liveSubscription = this.signalrService.notification$.subscribe({
      next: (liveNotification: NotificationDto) => {
        // Unshift adds the newest message card directly to the top of the feed layout stack
        this.rawNotifications.update(currentList => [liveNotification, ...currentList]);
      }
    });
  }

  loadHistoricalNotifications(): void {
    this.isLoading.set(true);
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        // Sort chronologically by newest date entries descending
        const sorted = (data || []).sort((a, b) => 
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.rawNotifications.set(sorted);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  setFilter(filter: FilterType): void {
    this.activeFilter.set(filter);
  }
  readSingleNotification(notification: NotificationDto): void {
    if (notification.isRead) return;

    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        this.rawNotifications.update(list => 
          list.map(n => n.id === notification.id ? { ...n, isRead: true } : n)
        );
      },
      error: (err) => console.error('Failed to update notification reading milestone state:', err)
    });
  }
  markAllAsRead(): void {
    if (this.unreadCount() === 0) return;
    
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        // Update all items in the UI state to read matching your database state execution
        this.rawNotifications.update(list => 
          list.map(n => ({ ...n, isRead: true }))
        );
      }
    });
  }

  getIconName(type: string): string {
    switch(type?.toLowerCase()) {
      case 'alert': return 'error_outline';
      case 'success': return 'check_circle_outline';
      case 'reminder': return 'info';
      default: return 'info';
    }
  }

  getRelativeTime(dateString: string): string {
    const past = new Date(dateString);
    const now = new Date();
    const msPerMinute = 60 * 1000;
    const msPerHour = msPerMinute * 60;
    const msPerDay = msPerHour * 24;
    const elapsed = now.getTime() - past.getTime();

    if (elapsed < msPerMinute) return 'Just now';
    if (elapsed < msPerHour) return Math.round(elapsed / msPerMinute) + ' minutes ago';
    if (elapsed < msPerDay) return Math.round(elapsed / msPerHour) + ' hours ago';
    
    const days = Math.round(elapsed / msPerDay);
    return days === 1 ? 'Yesterday' : `${days} days ago`;
  }
  clearAllNotifications(): void {
    if (this.rawNotifications().length === 0) return;

    this.notificationService.clearAllNotifications().subscribe({
      next: () => {
        // Flush out arrays locally to drop elements off-screen immediately
        this.rawNotifications.set([]);
      },
      error: (err) => console.error('Failed to clear clinical feed stack:', err)
    });
  }
  ngOnDestroy(): void {
    if (this.liveSubscription) {
      this.liveSubscription.unsubscribe();
    }
  }
}