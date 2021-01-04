import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ClientdetailComponent } from './clientdetail/clientdetail.component';
import { ClientsComponent } from './clients/clients.component';
import { LoginComponent } from './login/login.component';

import { WelcomeComponent } from './welcome/welcome.component';
const routes: Routes = [
  { path:'',component: LoginComponent},
  { path:'welcome-component',component:WelcomeComponent,children:[   
  ]},
  { path:'clients-component',component:ClientsComponent},
  { path:'clientdetail-component',component:ClientdetailComponent} 
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
