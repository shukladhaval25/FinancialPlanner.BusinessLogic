import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders} from '@angular/common/http';
import { user } from 'src/app/classes/user';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  userobj: user;
  constructor(private httpClient: HttpClient) {
    this.userobj = new user;
   }
  

  authenticateUserCredential(username : string, pass:string) : Observable<any>{ 
    try{
        //  this.userobj.UserName = username;
        //  this.userobj.Password = pass;  // Cy3r/i+CEClzGnbgrJx3QQ==
        //  this.userobj.MachineName = "";

      let headers = new HttpHeaders();     
      return  this.httpClient.post(
        "http://localhost:8081/FinancialPlanner/api/Authentication/AuthenticateClient",
        "{\"UserName\":\"" + username + "\",\"Password\":\"" + pass +"\"}",
        {headers: {
          'Access-Control-Allow-Origin':'*',
          'Access-Control-Allow-Methods':'*',
          'Access-Control-Allow-Headers':'*',
          'Content-Type': 'application/json',
          'Accept':'application/json'           
        }});
      //return this.httpClient.get("http://dummy.restapiexample.com/api/v1/employees");
    }
    catch (Error)   
    {  
      alert(Error.message);  
    }  
  }
}
