import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from 'src/services/authentication.service';
import { user } from '../classes/user';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html',
  styleUrls: ['./welcome.component.css']
})
export class WelcomeComponent implements OnInit {

  userobj: user;
 
  constructor(private _authenticateService:AuthenticationService,private router: Router) {
    // this.route.queryParams.subscribe(params => {
    //   if (params && params.special) {
    //     this.username = JSON.parse(params.special);
    //   }
    // });
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

  onClientSearchClick(){
    console.log("Clients component clicked");
    this.router.navigate([`/clients-component`]);
  }
  onTaskClick(){
    console.log("Task clicked");
    this.router.navigate([`/task-component`]);
  }

  // onLogoutClick(){
  //   console.log("Logout called");
  //   this._authenticateService.userobj = undefined;
  //   this.router.navigate(['']);
  // }

}
