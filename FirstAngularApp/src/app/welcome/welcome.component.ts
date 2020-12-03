import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html',
  styleUrls: ['./welcome.component.css']
})
export class WelcomeComponent implements OnInit {

  constructor(private router: Router) {}

  ngOnInit(): void {
  }
  
  onClientSearchClick(){
    console.log("Client search module clicked");
    this.router.navigate([`/clientsearch-component`]);
  }
  onTaskClick(){
    console.log("Task clicked");
    this.router.navigate([`/task-component`]);
  }
}
