import { Component, OnInit, signal, computed, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';
import { NotificationDto } from '../../models/notification.model';
import { PatientNotificationService } from '../../services/patient-notification.service';
import { SignalrService } from '../../services/signalr.service';
import { AuthService } from '../../core/services/auth.service';
import { Router, RouterModule } from '@angular/router';

export type DoctorFilterType = 'ALL' | 'UNREAD' | 'ALERTS';

@Component({
  selector: 'app-doctor-notification',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './doctor-notifications.component.html',
  styleUrls: ['./doctor-notifications.component.css']
})
export class DoctorNotificationsComponent implements OnInit, OnDestroy {
  private notificationService = inject(PatientNotificationService);
  private signalrService = inject(SignalrService);
  private liveSubscription!: Subscription;
  private router      = inject(Router);
  private authService = inject(AuthService);
  isLoading = signal<boolean>(true);
  rawNotifications = signal<NotificationDto[]>([]);
  activeFilter = signal<DoctorFilterType>('ALL');

  // Compute stats reactively
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

    // Catch real-time point-of-care patient alerts fired down by the server
    this.liveSubscription = this.signalrService.notification$.subscribe({
      next: (liveNotification: NotificationDto) => {
        this.rawNotifications.update(currentList => [liveNotification, ...currentList]);
      }
    });
  }

  loadHistoricalNotifications(): void {
    this.isLoading.set(true);
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        const sorted = (data || []).sort((a, b) => 
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.rawNotifications.set(sorted);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  } 
  readSingleNotification(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe({
        next: () => {
          this.rawNotifications.update(list =>
            list.map(n => n.id === notification.id ? { ...n, isRead: true } : n)
          );
          this.navigateToSource(notification);
        },
        error: () => this.navigateToSource(notification)
      });
    } else {
      this.navigateToSource(notification);
    }
  }
  setFilter(filter: DoctorFilterType): void {
    this.activeFilter.set(filter);
  }

  markAllAsRead(): void {
    if (this.unreadCount() === 0) return;
    
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
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
      case 'reminder':
      case 'info': 
        return 'info_outline';
      default: return 'info_outline';
    }
  }

  getRelativeTime(dateString: string): string {
    const past = new Date(dateString);
    const now = new Date();
    const elapsed = now.getTime() - past.getTime();

    const msPerMinute = 60 * 1000;
    const msPerHour = msPerMinute * 60;
    const msPerDay = msPerHour * 24;

    if (elapsed < msPerMinute) return 'Just now';
    if (elapsed < msPerHour) return Math.round(elapsed / msPerMinute) + 'm ago';
    if (elapsed < msPerDay) return Math.round(elapsed / msPerHour) + 'h ago';
    
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
  private navigateToSource(notification: NotificationDto): void {
    if (!notification.referenceType) return;
  
    switch (notification.referenceType) {
      case 'appointment':
        this.router.navigate(['/doctor/appointments']);
        break;
  
        case 'document':
          this.router.navigate(['/doctor/my-patients', notification.referenceId], {
            queryParams: { tab: 'documents' }
          });
          break;
  
      case 'health':
        this.router.navigate(['/doctor/my-patients']);
        break;
  
      case 'connection':
        this.router.navigate(['/doctor/my-patients']);
        break;
  
      case 'invoice':
        this.router.navigate(['/doctor/invoices', notification.referenceId]);
        break;
  
      case 'refund':
        this.router.navigate(['/doctor/invoices', notification.referenceId]);
        break;
      case 'QuestionnaireSubmission':
        this.router.navigate(['/doctor/my-patients', notification.referenceId], {
          queryParams: { tab: 'questionnaires' } // <-- This forces the Questionnaires tab to open
        });
                  break;
      default:
        break;
    }
  }
}