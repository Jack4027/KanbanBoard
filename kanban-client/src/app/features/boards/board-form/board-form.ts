import { Component, Inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { CreateBoardDto } from '../../../shared/models/board.model';

@Component({
  selector: 'app-board-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './board-form.html',
  styleUrl: './board-form.scss'
})
export class BoardFormComponent {
  boardForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private dialogRef: MatDialogRef<BoardFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.boardForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]]
    });
  }

  onSubmit(): void {
    if (this.boardForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    const dto: CreateBoardDto = this.boardForm.value;

    this.http.post(`${environment.apiUrl}/boards`, dto).subscribe({
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

  get name() { return this.boardForm.get('name'); }
}