import { EventEmitter, Input, NgModule, Output } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Router, Routes } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})

export class LoginComponent implements OnInit {
  [x: string]: any;
  
  constructor(private router: Router) {}
  displayAlert:boolean=true;
  @Input() strUserName : string;
  @Output() cdMsgChange = new EventEmitter<string>();  

  @Input() strPassword : string;
  @Output() cdPasswordChange = new EventEmitter<string>();
 
  ngOnInit(): void {
  }

  onLogin(){
    if (this.strUserName ==="Dhaval"  && this.strPassword === "Dhaval01")
    {
      this.displayAlert = true;
      console.log('Login success');
      this.router.navigate([`/welcome-component`]);
    }
    else{
      this.displayAlert = false;
    }
  }
  onAlertClose(){
    this.displayAlert = true;
  }

  updateUserName(val : string) {
    this.strUserName = val;
    this.cdMsgChange.emit(this.strUserName);
  }	

  updatePassword(val : string) {
    this.strPassword = val;
    this.cdMsgChange.emit(this.strUserName);
  }	
}
