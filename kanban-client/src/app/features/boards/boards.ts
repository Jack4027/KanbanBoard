import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { environment } from '../../../environments/environment';
import { BoardSummary } from '../../shared/models/board.model';
import { BoardFormComponent } from './board-form/board-form';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-boards',
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './boards.html',
  styleUrl: './boards.scss'
})
export class BoardsComponent implements OnInit {
  boards: BoardSummary[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBoards();
  }

  loadBoards(): void {
    this.isLoading = true;
    this.http.get<BoardSummary[]>(`${environment.apiUrl}/boards`).subscribe({
      next: boards => {
        this.boards = [...boards];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.errorMessage = err.message;
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openBoard(id: string): void {
    this.router.navigate(['/boards', id]);
  }

createBoard(): void {
  const dialogRef = this.dialog.open(BoardFormComponent, {
    width: '400px'
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.loadBoards();
    }
  });
}

deleteBoard(id: string, event: MouseEvent): void {
  event.stopPropagation();

  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    width: '400px',
    data: {
      title: 'Delete Board',
      message: 'Are you sure you want to delete this board? All columns and cards will be permanently deleted.',
      confirmText: 'Delete',
      confirmColor: 'warn'
    } as ConfirmDialogData
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.http.delete(`${environment.apiUrl}/boards/${id}`).subscribe({
        next: () => {
          this.loadBoards();
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.errorMessage = err.message;
          this.cdr.detectChanges();
        }
      });
    }
  });
}
}