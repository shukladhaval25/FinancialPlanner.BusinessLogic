import { HttpClient } from '@angular/common/http';
import { stringify } from '@angular/compiler/src/util';
import { EventEmitter, Input, Output } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ClientService } from 'src/services/client.service';
import { client } from '../classes/client';

@Component({
  selector: 'app-clientdetail',
  templateUrl: './clientdetail.component.html',
  styleUrls: ['./clientdetail.component.css']
})
export class ClientdetailComponent implements OnInit {
  [x: string]: any;

  p:number = 1;

  @Input() clientname : string;
  @Output() cdClientNameChange = new EventEmitter<string>();  

  @Input() adharCardNumber : string;
  @Output() cdAdharCardNumber = new EventEmitter<string>();



  singleClient:client;
  constructor(private clientservice:ClientService,private router: Router,private httpClient:HttpClient) { }

  ngOnInit(): void {
    this.singleClient = this.clientservice.singleclient;
  }

  onBackFromClientInformation(){
    this.router.navigate([`/clients-component`]);
  }
  onSaveClientInformation(){
    let obj:object;
    this.clientservice.saveClient().subscribe(data => {
      obj = data.Value});

    // let pannumber:String;
    // pannumber = this.singleClient.PAN;
    // // this.httpClient.post(
    // //   "http://localhost:8081/FinancialPlanner/api/Client/Add",
    // //     "{\"ID\":\"1\",\"Name\":\"" + this.clientname +"\",\"Adhar\":\"" +
    // //       this.sadharCardNumber + "\",\"PAN\":\"" + pannumber +"\"}",
    // //     {headers: {
    // //       'Access-Control-Allow-Origin':'*',
    // //       'Access-Control-Allow-Methods':'*',
    // //       'Access-Control-Allow-Headers':'*',
    // //       'Content-Type': 'application/json',
    // //       'Accept':'application/json'           
    // //     }});
    // this.httpClient.get("http://localhost:8081/FinancialPlanner/api/client/get");
  }

  
 updateclientName(val : string) {
  this.clientname = val;
  this.cdClientNameChange.emit(this.clientname);
  this.clientservice.singleclient.Name = val;
}	

updateAdharNumber(val : string) {
  this.adharCardNumber = val;
  this.cdAdharCardNumber.emit(this.adharCardNumber);
  this.clientservice.singleclient.Aadhar = val;
}	
}
