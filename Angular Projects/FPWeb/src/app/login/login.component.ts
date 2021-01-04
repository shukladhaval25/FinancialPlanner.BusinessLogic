import { EventEmitter, Input, Output } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from 'src/services/authentication.service';
import { user } from '../classes/user';
import { NavigationExtras, Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  [x: string]: any;

  ngOnInit() { 
      
  }

  //userobj : user;
  @Input() strUserName : string;
  @Output() cdMsgChange = new EventEmitter<string>();  

  @Input() strPassword : string;
  @Output() cdPasswordChange = new EventEmitter<string>();
 
  constructor(private _authenticateService:AuthenticationService, private router: Router,) {
   
  }

  onLogin(){
    let userobj = new user(); 
    this.strUserName = "Admin";
    this.strPassword = "Cy3r/i+CEClzGnbgrJx3QQ==";
      this._authenticateService.authenticateUserCredential(this.strUserName,this.strPassword).subscribe( data => {
      this._authenticateService.userobj = data.Value;
     console.log("User Name: " + this._authenticateService.userobj.FirstName); 
    //  if (this._authenticateService.userobj === undefined){
    //    this._authenticateService.userobj = new user();
    //  }
    //  this._authenticateService.userobj = userobj;
    //  let navigationExtras: NavigationExtras = {
    //   queryParams: {
    //     special: JSON.stringify(this.userobj.FirstName)
    //   }
    // };
     this.router.navigate([`/welcome-component`]);
     //this.router.navigate(['details'], navigationExtras);
   });
 }
 onAlertClose(){
   //this.displayAlert = true;
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
