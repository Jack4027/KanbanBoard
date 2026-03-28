import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChildren, QueryList } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { CdkDropList, DragDropModule, CdkDragDrop, CdkDropListGroup, transferArrayItem, moveItemInArray } from '@angular/cdk/drag-drop';
import { environment } from '../../../environments/environment';
import { Board } from '../../shared/models/board.model';
import { Column } from '../../shared/models/column.model';
import { Card } from '../../shared/models/card.model';
import { SignalRService } from '../../core/services/signalr.service';
import { ColumnFormComponent, ColumnFormData } from './column-form/column-form';
import { CardFormComponent, CardFormData } from './card-form/card-form';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/confirm-dialog/confirm-dialog';
import { CdkScrollableModule } from '@angular/cdk/scrolling';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    DragDropModule,
    CdkDropListGroup,
    CdkScrollableModule
  ],
  templateUrl: './board-detail.html',
  styleUrl: './board-detail.scss'
})
export class BoardDetailComponent implements OnInit, OnDestroy {
  board: Board | null = null;
  isLoading = true;
  errorMessage = '';
  boardId = '';

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private dialog: MatDialog,
    private signalRService: SignalRService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.boardId = this.route.snapshot.paramMap.get('id')!;
    this.loadBoard();
    this.startSignalR();
  }

  ngOnDestroy(): void {
    this.signalRService.offCardMoved();
    this.signalRService.offCardCreated();
    this.signalRService.offCardDeleted();
    this.signalRService.leaveBoard(this.boardId);
    this.signalRService.stopConnection();
  }

  @ViewChildren(CdkDropList) dropLists!: QueryList<CdkDropList>;

  loadBoard(): void {
    this.isLoading = true;
    this.http.get<Board>(`${environment.apiUrl}/boards/${this.boardId}`).subscribe({
      next: board => {
        this.board = board;
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

  async startSignalR(): Promise<void> {
    try {
      await this.signalRService.startConnection();
      await this.signalRService.joinBoard(this.boardId);

      this.signalRService.onCardMoved(card => {
        if (!this.board) return;

        this.board.columns.forEach(col => {
          col.cards = col.cards.filter(c => c.id !== card.id);
        });

        const targetColumn = this.board.columns.find(col => col.id === card.columnId);
        if (targetColumn) {
          targetColumn.cards.push(card);
        }

        this.cdr.detectChanges();
      });

      this.signalRService.onCardCreated(card => {
        if (!this.board) return;

        const column = this.board.columns.find(col => col.id === card.columnId);
        if (column) {
          column.cards.push(card);
          this.cdr.detectChanges();
        }
      });

      this.signalRService.onCardDeleted((cardId, columnId) => {
        if (!this.board) return;

        const column = this.board.columns.find(col => col.id === columnId);
        if (column) {
          column.cards = column.cards.filter(c => c.id !== cardId);
          this.cdr.detectChanges();
        }
      });

    } catch (err) {
      console.error('SignalR connection failed', err);
    }
  }

  onCardDrop(event: CdkDragDrop<Card[]>, targetColumn: Column): void {
      if (event.previousContainer === event.container) {
    moveItemInArray(
      event.container.data,
      event.previousIndex,
      event.currentIndex
    );
    this.cdr.detectChanges();
    return;
  }
    const card = event.previousContainer.data[event.previousIndex];

    if (card.columnId === targetColumn.id) return;

  transferArrayItem(
    event.previousContainer.data,
    event.container.data,
    event.previousIndex,
    event.currentIndex
  );

    card.columnId = targetColumn.id;
    this.cdr.detectChanges();

    this.http.post(`${environment.apiUrl}/cards/${card.id}/move`, {
      targetColumnId: targetColumn.id
    }).subscribe({
      error: (err: Error) => {
        this.errorMessage = err.message;
        this.loadBoard();
      }
    });
  }

getDropLists(): CdkDropList[] {
  return this.dropLists ? this.dropLists.toArray() : [];
}

  openAddColumn(): void {
    const dialogRef = this.dialog.open(ColumnFormComponent, {
      width: '400px',
      data: { boardId: this.boardId } as ColumnFormData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadBoard();
    });
  }

  openEditColumn(column: Column, event: MouseEvent): void {
    event.stopPropagation();
    const dialogRef = this.dialog.open(ColumnFormComponent, {
      width: '400px',
      data: { boardId: this.boardId, column } as ColumnFormData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadBoard();
    });
  }

  deleteColumn(columnId: string, event: MouseEvent): void {
    event.stopPropagation();
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Column',
        message: 'Are you sure you want to delete this column? All cards will be permanently deleted.',
        confirmText: 'Delete',
        confirmColor: 'warn'
      } as ConfirmDialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.http.delete(`${environment.apiUrl}/columns/${columnId}`).subscribe({
          next: () => this.loadBoard(),
          error: (err: Error) => this.errorMessage = err.message
        });
      }
    });
  }

  openAddCard(columnId: string): void {
    const dialogRef = this.dialog.open(CardFormComponent, {
      width: '500px',
      data: { columnId } as CardFormData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadBoard();
    });
  }

  openEditCard(card: Card, event: MouseEvent): void {
    event.stopPropagation();
    const dialogRef = this.dialog.open(CardFormComponent, {
      width: '500px',
      data: { columnId: card.columnId, card } as CardFormData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadBoard();
    });
  }

  deleteCard(card: Card, event: MouseEvent): void {
    event.stopPropagation();
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Card',
        message: 'Are you sure you want to delete this card?',
        confirmText: 'Delete',
        confirmColor: 'warn'
      } as ConfirmDialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.http.delete(`${environment.apiUrl}/cards/${card.id}`).subscribe({
          next: () => this.loadBoard(),
          error: (err: Error) => this.errorMessage = err.message
        });
      }
    });
  }
}