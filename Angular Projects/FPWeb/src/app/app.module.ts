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
import { TopNavComponent } from './top-nav/top-nav.component';

import {Ng2SearchPipeModule} from 'ng2-search-filter';
import { FormsModule } from '@angular/forms';

import {NgxPaginationModule} from 'ngx-pagination';

// import { MatIconModule } from "@angular/material/icon";
// import { MatListModule } from "@angular/material/list";
// import { MatSidenavModule } from "@angular/material/sidenav";
// import { MatToolbarModule } from "@angular/material/toolbar";
// import { MatButtonModule } from "@angular/material/button";

// import { FlexLayoutModule } from "@angular/flex-layout";
// import { MenuListItemComponent } from "./menu-list-item/menu-list-item.component";

@NgModule({
  declarations: [
    AppComponent,
    WelcomeComponent,
    LoginComponent,
    ClientsComponent,
    LoggeedinuserComponent,
    ClientdetailComponent,
    TopNavComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    Ng2SearchPipeModule,
    FormsModule,
    NgxPaginationModule
  ],
  providers: [AuthenticationService,ClientService],
  bootstrap: [AppComponent]
})
export class AppModule { }
