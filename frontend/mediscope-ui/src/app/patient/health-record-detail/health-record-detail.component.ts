import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HealthHistoryService } from '../../services/health-history.service';

@Component({
  selector: 'app-health-record-detail',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, RouterLink],
  templateUrl: './health-record-detail.component.html',
  styleUrl: './health-record-detail.component.css'
})
export class HealthRecordDetailComponent implements OnInit {
  // ── 🛠️ INJECT ACTIVATED ROUTE TO READ URL ──
  private route = inject(ActivatedRoute);
  private historyService = inject(HealthHistoryService);

  recordId = signal<string | null>(null);

  ngOnInit(): void {
    // ── 🛠️ EXTRACT THE ID FROM THE URL ──
    this.route.paramMap.subscribe(params => {
      const id = params.get('id'); // 'id' matches the ':id' defined in app.routes.ts
      
      if (id) {
        this.recordId.set(id);
        this.fetchSpecificRecord(id);
      }
    });
  }

  fetchSpecificRecord(id: string): void {
    // Execute backend call to fetch the single submission details
    // e.g., this.historyService.getRecordById(id).subscribe(...)
  }
}