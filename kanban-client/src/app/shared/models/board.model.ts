import { BoardMember } from "./board-member.model";
import { Column } from "./column.model";

export interface Board {
  id: string;
  name: string;
  createdBy: string;
  createdAt: string;
  columns: Column[];
  members: BoardMember[];
}

export interface BoardSummary {
  id: string;
  name: string;
  createdBy: string;
  createdAt: string;
  columnCount: number;
  memberCount: number;
}

export interface CreateBoardDto {
  name: string;
}

export interface UpdateBoardDto {
  name: string;
}