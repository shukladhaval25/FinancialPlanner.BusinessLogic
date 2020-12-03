import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClientsearchComponent } from './clientsearch.component';

describe('ClientsearchComponent', () => {
  let component: ClientsearchComponent;
  let fixture: ComponentFixture<ClientsearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ClientsearchComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ClientsearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
