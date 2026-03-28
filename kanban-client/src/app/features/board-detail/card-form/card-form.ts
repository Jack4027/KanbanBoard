import { Component, Inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Card } from '../../../shared/models/card.model';

export interface CardFormData {
  columnId: string;
  card?: Card;
}

@Component({
  selector: 'app-card-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './card-form.html',
  styleUrl: './card-form.scss'
})
export class CardFormComponent {
  cardForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  isEditMode = false;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private dialogRef: MatDialogRef<CardFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CardFormData
  ) {
    this.isEditMode = !!data.card;

    this.cardForm = this.fb.group({
      title: [data.card?.title ?? '', [Validators.required, Validators.maxLength(200)]],
      description: [data.card?.description ?? '']
    });
  }

  onSubmit(): void {
    if (this.cardForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    const request$ = this.isEditMode
      ? this.http.put(`${environment.apiUrl}/cards/${this.data.card!.id}`, this.cardForm.value)
      : this.http.post(`${environment.apiUrl}/columns/${this.data.columnId}/cards`, this.cardForm.value);

    request$.subscribe({
      next: () => {
        this.isLoading = false;
        this.dialogRef.close(true);
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.isLoading = false;
        this.errorMessage = err.message;
        this.cdr.detectChanges();
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  get title() { return this.cardForm.get('title'); }
}