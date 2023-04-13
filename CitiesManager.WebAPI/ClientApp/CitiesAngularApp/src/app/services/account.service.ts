import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { LoginUser } from '../models/login-user';
import { RegisterUser } from '../models/register-user';

const API_BASE_URL = "https:/localhost:7017/api/";
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  public currentUsername: string | null = null;

  constructor(private httpClient: HttpClient) {
  }

  public RegisterUser(registerUser: RegisterUser): Observable<any> {
    return this.httpClient.post<any>(`${API_BASE_URL}v1/Account/register`, registerUser);
  }

  public LoginUser(loginUser: LoginUser): Observable<any> {
    return this.httpClient.post<any>(`${API_BASE_URL}v1/Account/login`, loginUser);
  }

  public LogoutUser(): Observable<string> {
    return this.httpClient.get<string>(`${API_BASE_URL}v1/Account/logout`);
  }
}
