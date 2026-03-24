import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddAlbum } from './add-album';

describe('AddAlbum', () => {
  let component: AddAlbum;
  let fixture: ComponentFixture<AddAlbum>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddAlbum]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddAlbum);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
