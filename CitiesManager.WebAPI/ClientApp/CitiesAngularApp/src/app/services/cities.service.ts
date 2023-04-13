import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";

import { City } from "../models/city";

const API_BASE_URL = "https:/localhost:7017/api/";

@Injectable({
  providedIn: 'root'
})
export class CitiesService {

  cities: City[] = [];

  constructor(private httpClient: HttpClient) {

  }

  public getCities(): Observable<City[]> {
    let headers = new HttpHeaders();
    headers = headers.append("Authorization", `Bearer ${localStorage['token']}`);

    return this.httpClient.get<City[]>(`${API_BASE_URL}v1/cities`, {
      headers: headers
    });
  }

  public postCity(city: City): Observable<City> {
    let headers = new HttpHeaders();
    headers = headers.append("Authorization", `Bearer ${localStorage['token']}`);

    return this.httpClient.post<City>(`${API_BASE_URL}v1/cities`,
      city,
      { headers: headers }
    );
  }

  public putCity(city: City): Observable<string> {
    let headers = new HttpHeaders();
    headers = headers.append("Authorization", `Bearer ${localStorage['token']}`);

    return this.httpClient.put<string>(`${API_BASE_URL}v1/cities/${city.cityID}`,
      city,
      { headers: headers }
    );
  }

  public deleteCity(city: City): Observable<string> {
    let headers = new HttpHeaders();
    headers = headers.append("Authorization", `Bearer ${localStorage['token']}`);

    return this.httpClient.delete<string>(`${API_BASE_URL}v1/cities/${city.cityID}`,
      { headers: headers }
    );
  }
}
