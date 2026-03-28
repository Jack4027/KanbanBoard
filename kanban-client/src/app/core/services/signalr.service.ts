import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { Card } from '../../shared/models/card.model';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;

  constructor(private authService: AuthService) {}

  async startConnection(): Promise<void> {
    
  this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(environment.hubUrl, {
      accessTokenFactory: () => this.authService.getToken() ?? ''
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Debug)
    .build();

  await this.hubConnection.start();
  }

  async joinBoard(boardId: string): Promise<void> {
    await this.hubConnection?.invoke('JoinBoard', boardId);
  }

  async leaveBoard(boardId: string): Promise<void> {
    await this.hubConnection?.invoke('LeaveBoard', boardId);
  }

  onCardMoved(callback: (card: Card) => void): void {
    this.hubConnection?.on('CardMoved', callback);
  }

  onCardCreated(callback: (card: Card) => void): void {
    this.hubConnection?.on('CardCreated', callback);
  }

  onCardDeleted(callback: (cardId: string, columnId: string) => void): void {
    this.hubConnection?.on('CardDeleted', callback);
  }

  offCardMoved(): void {
    this.hubConnection?.off('CardMoved');
  }

  offCardCreated(): void {
    this.hubConnection?.off('CardCreated');
  }

  offCardDeleted(): void {
    this.hubConnection?.off('CardDeleted');
  }

  async stopConnection(): Promise<void> {
    await this.hubConnection?.stop();
    this.hubConnection = null;
  }
}