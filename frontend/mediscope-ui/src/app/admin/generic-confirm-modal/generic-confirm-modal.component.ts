import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText: string;
  cancelText?: string;
  theme: 'primary' | 'warning' | 'danger';
  icon?: string;
}

@Component({
  selector: 'app-generic-confirm-modal',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './generic-confirm-modal.component.html',
  styleUrls: ['./generic-confirm-modal.component.css']
})
export class GenericConfirmModalComponent {
  constructor(
    public dialogRef: MatDialogRef<GenericConfirmModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}
}