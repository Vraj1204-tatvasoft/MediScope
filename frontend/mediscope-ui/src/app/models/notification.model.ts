export interface NotificationDto {

    id: string;
  
    type:
      'alert'
      | 'info'
      | 'success'
      | 'reminder';
  
    message: string;
  
    isRead: boolean;
  
    createdAt: string;
  
    readAt?: string;
  }