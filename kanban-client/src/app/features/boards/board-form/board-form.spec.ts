import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BoardFormComponent } from './board-form';

describe('BoardForm', () => {
  let component: BoardFormComponent;
  let fixture: ComponentFixture<BoardFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoardFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BoardFormComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
