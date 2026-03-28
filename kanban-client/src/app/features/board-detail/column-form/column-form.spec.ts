import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ColumnFormComponent } from './column-form';

describe('ColumnForm', () => {
  let component: ColumnFormComponent;
  let fixture: ComponentFixture<ColumnFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ColumnFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ColumnFormComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
