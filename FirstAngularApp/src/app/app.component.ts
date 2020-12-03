import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'FirstAngularApp';
  isLoginDisplay: boolean = false;
  constructor(private router: Router) {}

  ngOnInit(): void {
    
  }

  onShowLogin(){
    this.isLoginDisplay = false;
    this.router.navigate([`/login-component`]);    
  }
}
