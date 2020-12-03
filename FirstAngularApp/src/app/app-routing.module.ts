import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ClientsearchComponent } from './clientsearch/clientsearch.component';
import { TaskComponent } from './task/task.component';
import { WelcomeComponent } from './welcome/welcome.component';

const routes: Routes = [
  /*{ path: '', component: LoginComponent },*/
  { path: '', component: WelcomeComponent, children:[
    {path:'clientsearch-component',component: ClientsearchComponent},
    {path:'task-component',component: TaskComponent},
  ]}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
