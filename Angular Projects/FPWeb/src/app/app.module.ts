import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import {HttpClientModule} from '@angular/common/http';
import { AuthenticationService } from 'src/services/authentication.service';
import { WelcomeComponent } from './welcome/welcome.component';
import { LoginComponent } from './login/login.component';
import { ClientsComponent } from './clients/clients.component';
import { LoggeedinuserComponent } from './loggeedinuser/loggeedinuser.component';
import { ClientService } from 'src/services/client.service';
import { ClientdetailComponent } from './clientdetail/clientdetail.component';

@NgModule({
  declarations: [
    AppComponent,
    WelcomeComponent,
    LoginComponent,
    ClientsComponent,
    LoggeedinuserComponent,
    ClientdetailComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [AuthenticationService,ClientService],
  bootstrap: [AppComponent]
})
export class AppModule { }
