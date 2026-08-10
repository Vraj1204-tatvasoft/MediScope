import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { DoctorPatientResponseDto } from '../models/doctor-patient,model';
import { NotificationDto } from '../models/notification.model';
import { DoctorPatientService } from './doctor-patient.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection: signalR.HubConnection | null = null;
  
  // Create an internal stream dispatcher to push events cleanly to components
  private incomingRequestSubject = new Subject<DoctorPatientResponseDto>();
  public incomingRequest$: Observable<DoctorPatientResponseDto> = this.incomingRequestSubject.asObservable();
  private requestUpdateSubject = new Subject<any>();
  public requestUpdate$ = this.requestUpdateSubject.asObservable();
  private notificationSubject = new Subject<NotificationDto>();
  public notification$ =  this.notificationSubject.asObservable();
  private forceLogoutSubject = new Subject<{ reason: string }>();
  public forceLogout$ = this.forceLogoutSubject.asObservable();
  private dashboardUpdatedSubject = new Subject<void>();
  public dashboardUpdated$ = this.dashboardUpdatedSubject.asObservable();
  /* Initializes WebSocket handshakes securely targeting user authorization claims */
  private broadcastStatusSubject = new Subject<any>();
  public broadcastStatus$ = this.broadcastStatusSubject.asObservable();
  private broadcastProgressSubject = new Subject<any>();
  public broadcastProgress$ = this.broadcastProgressSubject.asObservable();

  public startConnection(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    const token = localStorage.getItem('ms_access_token'); 

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5211/api/hubs/realtime', {
        accessTokenFactory: () => {
          const freshToken = localStorage.getItem('ms_access_token');
          return freshToken ? freshToken : '';
        }})
      .withAutomaticReconnect() 
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('MediScope Real-Time SignalR Engine Active.');
        this.registerServerEvents();
      })
      .catch(err => console.error('SignalR Initialization Fault:', err));
  }
  private dpService = inject(DoctorPatientService);
  /* Registers global callback listeners matching your C# hub invocations */
  private registerServerEvents(): void {
    this.hubConnection?.on('NewRequestIncoming',
      (data: DoctorPatientResponseDto) => {this.incomingRequestSubject.next(data);
        this.dpService.pendingRequestsCount.update(count => count + 1);
      });
    this.hubConnection?.on(
      'DoctorRequestUpdated',
      (data) => {
        console.log(
          'Doctor request updated:',
          data);
        this.requestUpdateSubject.next(data);
      });
    this.hubConnection?.on(
      'NotificationRecieved',
      (data: NotificationDto) => {
        console.log(
          'Realtime Notification:',
          data);
        this.notificationSubject.next(data);
      });
      this.hubConnection?.on('ForceLogout', (payload: { reason: string }) => {
        console.log('Force logout triggered:', payload);
        this.forceLogoutSubject.next(payload);
    });
    this.hubConnection?.on('DashboardUpdated', () => {
      console.log('DashboardUpdated received');
      this.dashboardUpdatedSubject.next();
    });
    this.hubConnection?.on('BroadcastStatusUpdated', (data) => {
      console.log('Broadcast Status Updated:', data);
      this.broadcastStatusSubject.next(data);
    });

    this.hubConnection?.on('BroadcastProgressUpdated', (data) => {
      console.log('Broadcast Progress Updated:', data);
      this.broadcastProgressSubject.next(data);
    });
    }

  /* Cleans up running background tasks upon system component logs or user logouts */
  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => {
        this.hubConnection = null;
        console.log('SignalR connection terminated safely.');
      });
    }
  }
}