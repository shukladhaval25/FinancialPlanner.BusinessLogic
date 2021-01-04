import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders} from '@angular/common/http';
import { user } from '../classes/client';

@Injectable({
  providedIn: 'root'
})
export class ClientService {

  constructor(private httpClient: HttpClient) { }
  
  getClients() : Observable<any>{ 
    try{
      //let headersObj = new HttpHeaders();
      let bodyStr = new String();
      let userobj = new user();
      userobj.UserName = "Admin";
      userobj.Password = "Cy3r/i+CEClzGnbgrJx3QQ==";

      bodyStr = "{{'UserName':'Admin'},{'Password':'Cy3r/i+CEClzGnbgrJx3QQ=='}}";

      //headersObj.set('Access-Control-Allow-Origin', '*');
      let headers = new Headers({ 'Content-Type': 'application/json' });
      headers.append('Accept', 'application/json');
      //let options = new RequestOptions({ headers: headers });
      
          //return  this.httpClient.get("http://localhost:8081/FinancialPlanner/api/client/get");
          //return  this.httpClient.get("http://localhost:8081/FinancialPlanner/api/user");
           return  this.httpClient.post("http://localhost:8081/FinancialPlanner/api/Authentication/AuthenticateClient",
           "{\"UserName\":\"Admin\",\"Password\":\"Cy3r/i+CEClzGnbgrJx3QQ==\"}",
           {headers: {
            'Access-Control-Allow-Origin':'*',
            'Access-Control-Allow-Methods':'*',
            'Access-Control-Allow-Headers':'*',
            'Content-Type': 'application/json',
            'Accept':'application/json'           
          }});
        //  return this.httpClient.post("http://localhost:8081/FinancialPlanner/api/Authentication/AuthenticateClient",
        //     JSON.stringify({userobj}) ,
        //    { headers:  {'Content-Type': 'application/json'} });
      //  return this.httpClient.post("http://localhost:8081/FinancialPlanner/api/Authentication/AuthenticateClient",
      //   userobj, { headers:  { 'Content-Type': 'application/x-www-form-urlencoded'} });
      //return this.httpClient.get("http://dummy.restapiexample.com/api/v1/employees");
    }
    catch (Error)   
    {  
      alert(Error.message);  
    }  
  }
}
