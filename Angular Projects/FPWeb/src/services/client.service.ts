import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { client } from 'src/app/classes/client';

@Injectable({
  providedIn: 'root'
})
export class ClientService {

  singleclient : client;
  constructor(private httpClient: HttpClient) { }
  
  getClients() : Observable<any>{ 
    try{
          return  this.httpClient.get("http://localhost:8081/FinancialPlanner/api/client/get");
    }
    catch (Error)   
    {  
      alert(Error.message);  
    }  
  }
}
