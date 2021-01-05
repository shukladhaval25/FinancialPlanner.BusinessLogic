import { HttpClient, HttpHandler } from '@angular/common/http';
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

  saveClient(): Observable<any>{
    console.log("Client service: Save client called");
    console.log("Client Name:" +  this.singleclient.Name);
    try{
      return this.httpClient.post(
        "http://localhost:8081/FinancialPlanner/api/Client/Update",
        "{\"ID\":\"" + this.singleclient.ID + "\",\"Name\":\"" + this.singleclient.Name +"\",\"Aadhar\":\"" +
          this.singleclient.Aadhar + "\",\"PAN\":\"" + this.singleclient.PAN +"\",\"DOB\":\"" +
          this.singleclient.DOB +"\",\"FatherName\":\"" + this.singleclient.FatherName +
          "\",\"MotheName\":\"" + this.singleclient.MotherName +
          "\",\"Gender\":\"" + this.singleclient.Gender +
          "\",\"PlaceOfBirth\":\"" + this.singleclient.PlaceOfBirth +
          "\",\"IsMarried\":\"" + this.singleclient.IsMarried +
          "\",\"MarriageAnniversary\":\"" + this.singleclient.MarriageAnniversary +
          "\",\"Occupation\":\"" + this.singleclient.Occupation +
          "\",\"imagePath\":\"" + this.singleclient.ImagePath +
          "\",\"UpdatedBy\":\"" + this.singleclient.UpdatedBy +
          "\"}",       
        {headers: {
          'Access-Control-Allow-Origin':'*',
          'Access-Control-Allow-Methods':'*',
          'Access-Control-Allow-Headers':'*',
          'Content-Type': 'application/json',
          'Accept':'application/json'           
        }});    
    }
    catch (Error)   
    {  
      alert(Error.message);  
    }  
  }
}
