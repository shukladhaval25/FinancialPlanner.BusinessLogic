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

  singleClient:client;
  constructor(private clientservice:ClientService,private router: Router) { }

  ngOnInit(): void {
    this.singleClient = this.clientservice.singleclient;
  }

  onBackFromClientInformation(){
    this.router.navigate([`/clients-component`]);
  }
}
