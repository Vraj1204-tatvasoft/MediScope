import { Component, signal, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import * as bootstrap from 'bootstrap';
import { SignalrService } from './services/signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('mediscope-ui');
  
  private signalrService = inject(SignalrService);
  ngOnInit(): void {
    const token = localStorage.getItem('ms_access_token');
    if (token) {
      console.log('Valid session discovered on app boot. Initializing real-time streams...');
    }
  }
  ngAfterViewInit() {
    const dropdownTriggerList = document.querySelectorAll('.dropdown-toggle');
    dropdownTriggerList.forEach(dropdownTriggerEl => {
      new bootstrap.Dropdown(dropdownTriggerEl);
    });
  }
}