import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from 'src/services/authentication.service';
import { ClientService } from 'src/services/client.service';
import { client } from '../classes/client';
import { user } from '../classes/user';

@Component({
  selector: 'app-clients',
  templateUrl: './clients.component.html',
  styleUrls: ['./clients.component.css']
})
export class ClientsComponent implements OnInit {
  [x: string]: any;

  clients: client[];
  
  constructor(private _clientService:ClientService,private router: Router) {  
    this.clients = [];
  }

  ngOnInit(): void {   
    console.log("Before API call");
    //  this._clientService.getClients().subscribe( data => {
    //   this.clients = data.Value;
    //  });
     this._clientService.getClients().subscribe(data =>{
         this.clients = data.Value;});
    console.log("After API call : "); 
    console.log("Total no of records: " + this.clients.length);        
  }

  onShowClientInformation(client:client){
     console.log("Link clicked");
     this._clientService.singleclient = client;
     this.router.navigate([`/clientdetail-component`]);   
    // console.log("Selected client Name:" + com.Name);
  }
}
