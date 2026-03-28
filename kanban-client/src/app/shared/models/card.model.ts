export interface Card {
  id: string;
  columnId: string;
  title: string;
  description?: string;
  createdAt: string;
}

export interface CreateCardDto {
  title: string;
  description?: string;
}

export interface UpdateCardDto {
  title: string;
  description?: string;
}

export interface MoveCardDto {
  targetColumnId: string;
}