import { Card } from "./card.model";

export interface Column {
  id: string;
  boardId: string;
  name: string;
  position: number;
  cards: Card[];
}

export interface CreateColumnDto {
  name: string;
}

export interface UpdateColumnDto {
  name: string;
}