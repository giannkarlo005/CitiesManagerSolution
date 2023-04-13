import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { Router } from '@angular/router';

import { AccountService } from '../services/account.service';
import { LoginUser } from '../models/login-user';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoginFormSubmitted: boolean = false;

  constructor(private accountService: AccountService, private router: Router) {
    this.loginForm = new FormGroup({
      email: new FormControl(null, [Validators.required, Validators.email]),
      password: new FormControl(null, [Validators.required])
    });
  }

  get login_emailControl(): any {
    return this.loginForm.controls["email"];
  }

  get login_passwordControl(): any {
    return this.loginForm.controls["password"];
  }

  loginUser() {
    this.isLoginFormSubmitted = true;
    if (!this.loginForm.valid) {
      return;
    }

    this.accountService.LoginUser(this.loginForm.value).subscribe({
      next: (response: any) => {
        localStorage["token"] = response.token;

        this.accountService.currentUsername = response.email;
        this.isLoginFormSubmitted = false;
        this.router.navigate(['/cities']);
        this.loginForm.reset();
      },
      error: (error: Error) => {
        console.log(error);
      },
      complete: () => { }
    });
  }
}
