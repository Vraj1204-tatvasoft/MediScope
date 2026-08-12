import { Component, computed, inject } from '@angular/core';
import { Router, NavigationEnd, RouterModule } from '@angular/router';
import { filter, map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/auth.model';
import { NAV_CONFIG } from '../../../core/constants/nav.config';
import { PatientNotificationService } from '../../../services/patient-notification.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  private router = inject(Router);
  private authService = inject(AuthService);
  private notificationService = inject(PatientNotificationService);

  // Today's date formatted
  today = new Date().toLocaleDateString('en-US', {
    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
  });

  user        = computed(() => this.authService.currentUser());
  userInitial = computed(() => this.user()?.fullName?.charAt(0).toUpperCase() ?? '');
  notificationCount = computed(() => this.notificationService.unreadCount());

  // Derive page title from current route matching nav config
  private currentUrl = toSignal(
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(e => (e as NavigationEnd).urlAfterRedirects)
    ),
    { initialValue: this.router.url }
  );

  pageTitle = computed(() => {
    const role = this.user()?.role as UserRole;
    const config = NAV_CONFIG[role];
  
    if (!config) {
      return 'Dashboard';
    }
  
    const url = this.currentUrl();
  
    for (const item of config.items) {
      if (item.route && url.startsWith(item.route)) {
        return item.label;
      }
  
      if (item.children) {
        const child = item.children.find(c =>
          c.route && url.startsWith(c.route)
        );
  
        if (child) {
          return child.label;
        }
      }
    }
  
    return 'Dashboard';
  });

  navigateToNotifications(): void {
    const role = this.user()?.role?.toLowerCase();
    if (role === 'doctor') {
      this.router.navigate(['/doctor/doctor-notifications']);
    } else if (role === 'patient'){
      this.router.navigate(['/patient/patient-notifications']);
    }
    else {
      this.router.navigate(['/admin/admin-notifications']);
    }
  }

  navigateToProfile(): void {
    const role = this.user()?.role?.toLowerCase();
    if (role === 'doctor') {
      this.router.navigate(['/doctor/profile']);
    } else {
      this.router.navigate(['/patient/profile']); 
    }
  }
}