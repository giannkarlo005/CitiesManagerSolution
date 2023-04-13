import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from './services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'CitiesAngularApp';

  constructor(public accountService: AccountService, private router: Router) { }

  onLogoutClicked() {
    this.accountService.LogoutUser().subscribe({
      next: () => {
        this.accountService.currentUsername = null;
        localStorage.removeItem('token');

        this.router.navigate(['/login'])
      },
      error: (error: Error) => {
        console.log(error);
      },
      complete: () => {

      }
    });
  }
}
