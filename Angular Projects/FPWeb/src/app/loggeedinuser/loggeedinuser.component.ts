import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from 'src/services/authentication.service';
import { user } from '../classes/user';

@Component({
  selector: 'app-loggeedinuser',
  templateUrl: './loggeedinuser.component.html',
  styleUrls: ['./loggeedinuser.component.css']
})
export class LoggeedinuserComponent implements OnInit {
  
  userobj: user;

  constructor(private _authenticateService:AuthenticationService,private router: Router) {
     console.log("Logged in user name :" + this._authenticateService.userobj.UserName);
    if (this._authenticateService.userobj === null || this._authenticateService.userobj.UserName === undefined)
    {
      console.log("Invalid session. Redirect to login");
      this.router.navigate(['']);      
    }
    else {
      this.userobj = _authenticateService.userobj;
    }
  }

  ngOnInit(): void {
  }

  onLogoutClick(){
    console.log("Logout called from logged in control");
    this._authenticateService.userobj = undefined;
    this.router.navigate(['']);
  }
}
