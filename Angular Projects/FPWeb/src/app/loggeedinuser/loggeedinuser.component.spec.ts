import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoggeedinuserComponent } from './loggeedinuser.component';

describe('LoggeedinuserComponent', () => {
  let component: LoggeedinuserComponent;
  let fixture: ComponentFixture<LoggeedinuserComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LoggeedinuserComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoggeedinuserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
