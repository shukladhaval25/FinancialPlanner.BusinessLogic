import { Component, OnInit } from '@angular/core';
import { ClientService } from '../services/client.service';
import { client, employees,user}  from '../classes/client';

@Component({
  selector: 'app-clientsearch',
  templateUrl: './clientsearch.component.html',
  styleUrls: ['./clientsearch.component.css']
})
export class ClientsearchComponent implements OnInit {

  constructor(private _clientService:ClientService) {
    this.empls = [];
    this.clients = [];
    this.users = new user;
  }

  empls: employees[];
  clients: client[];
  users: user;

  ngOnInit(): void {   
    console.log("Before API call");
    //  this._clientService.getClients().subscribe( data => {
    //   this.clients = data.Value;
    //  });
     this._clientService.getClients().subscribe(data =>{
         this.users = data.data;});
    console.log("After API call : "); 
    console.log("Total no of records: " + this.clients.length);        
  }

  callAPI(){
   
  }

}
