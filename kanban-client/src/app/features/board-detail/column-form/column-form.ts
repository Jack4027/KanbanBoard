import { Component, Inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Column } from '../../../shared/models/column.model';

export interface ColumnFormData {
  boardId: string;
  column?: Column;
}

@Component({
  selector: 'app-column-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './column-form.html',
  styleUrl: './column-form.scss'
})
export class ColumnFormComponent {
  columnForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  isEditMode = false;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private dialogRef: MatDialogRef<ColumnFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ColumnFormData
  ) {
    this.isEditMode = !!data.column;

    this.columnForm = this.fb.group({
      name: [data.column?.name ?? '', [Validators.required, Validators.maxLength(100)]]
    });
  }

  onSubmit(): void {
    if (this.columnForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    const request$ = this.isEditMode
      ? this.http.put(`${environment.apiUrl}/columns/${this.data.column!.id}`, this.columnForm.value)
      : this.http.post(`${environment.apiUrl}/boards/${this.data.boardId}/columns`, this.columnForm.value);

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

  get name() { return this.columnForm.get('name'); }
}