import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { Albums } from './albums';

describe('Albums', () => {
  let component: Albums;
  let fixture: ComponentFixture<Albums>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Albums],
      providers: [provideHttpClientTesting()]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Albums);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
