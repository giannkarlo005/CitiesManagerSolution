import { Component } from '@angular/core';
import { Form, FormArray, FormControl, FormGroup, Validators } from '@angular/forms';

import { City } from '../models/city';
import { AccountService } from '../services/account.service';
import { CitiesService } from '../services/cities.service';

@Component({
  selector: 'app-cities-component',
  templateUrl: './cities-component.component.html',
  styleUrls: ['./cities-component.component.css']
})
export class CitiesComponent {
  cities: City[] = [];
  postCityForm: FormGroup;
  putCityForm: FormGroup;

  editCityID: any;
  isPostCityFormSubmitted: boolean = false;

  constructor(private citiesService: CitiesService,
              private accountService: AccountService) {
    this.postCityForm = new FormGroup({
      cityName: new FormControl(null, [Validators.required])
    });

    this.putCityForm = new FormGroup({
      cities: new FormArray([])
    });

    this.editCityID = "";
  }

  loadCities() {
    this.citiesService.getCities()
      .subscribe({
        next: (response: City[]) => {
          this.cities = response;

          this.cities.forEach(city => {
            this.putCityFormArray.push(new FormGroup({
              cityID: new FormControl(city.cityID, [Validators.required]),
              cityName: new FormControl({ value: city.cityName, disabled: true }, [Validators.required])
            }));
          })
        },
        error: (error) => {
          console.log(error);
        },
        complete: () => { }
      });
  }

  ngOnInit() {
    this.loadCities();
  }

  get putCityFormArray() {
    return this.putCityForm.get("cities") as FormArray;
  }

  get postCity_CityNameControl(): any {
    return this.postCityForm.controls['cityName'];
  };

  public editCityClicked(city: City): void {
    this.editCityID = city.cityID;
  }

  public postCitySubmitted() {
    this.isPostCityFormSubmitted = true;

    console.log(this.postCityForm.value);

    this.citiesService.postCity(this.postCityForm.value).subscribe({
      next: (response: City) => {
        this.putCityFormArray.push(new FormGroup({
          cityID: new FormControl(response.cityID, [Validators.required]),
          cityName: new FormControl({ value: response.cityName, disabled: true }, [Validators.required])
        }));
        this.cities.push(new City(response.cityID, response.cityName));

        this.isPostCityFormSubmitted = false;
        this.postCityForm.reset();
      },
      error: (error: any) => {
        console.log(error);
      },
      complete: () => {

      }
    });
  }

  public updateClicked(i: number): void {
    this.citiesService.putCity(this.putCityFormArray.controls[i].value).subscribe({
      next: (response: string) => {
        this.editCityID = "";

        this.putCityFormArray.controls[i].reset(this.putCityFormArray.controls[i].value);
      },
      error: (error: any) => {
        console.log(error);
      },
      complete: () => {

      }
    });
  }

  public deleteCityClicked(city: City, i: number): void {
    if (confirm(`Are you sure to delete this city: ${city.cityName}`))
    this.citiesService.deleteCity(this.putCityFormArray.controls[i].value).subscribe({
      next: (response: string) => {
        this.putCityFormArray.removeAt(i);
        this.cities.splice(i, 1);
      },
      error: (error: any) => {
        console.log(error);
      },
      complete: () => {

      }
    });
  }

  refreshClicked(): void {
    this.accountService.postGenerateNewToken().subscribe({
      next: (response: any) => {
        localStorage["token"] = response.token;
        localStorage["refreshToken"] = response.refreshToken;

        this.loadCities();
      },
      error: (error: any) => {
        console.log(error);
      },
      complete: () => {
      },
    });
  }
}
